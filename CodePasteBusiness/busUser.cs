using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Westwind.BusinessFramework.LinqToSql;
using Westwind.Utilities;
using Manoli.Utils.CSharpFormat;
using System.Runtime.Serialization;


namespace CodePasteBusiness
{
    public class busUser : BusinessObjectLinq<User, CodePasteDataContext>
    {
        /// <summary>
        /// Override NewEntity to assign new ID immediately
        /// </summary>
        /// <returns></returns>
        public override User NewEntity()
        {
            if (base.NewEntity() == null)
                return null;

            Entity.Id = DataUtils.GenerateUniqueId();
            Entity.Entered = DateTime.Now;
            Entity.Updated = DateTime.Now;
            Entity.LastLanguage = string.Empty;
            Entity.Name = string.Empty;
            Entity.Email = string.Empty;
            Entity.Password = string.Empty;
            Entity.OpenId = string.Empty;
            Entity.OpenIdClaim = string.Empty;
            Entity.IsAdmin = false;
            Entity.Theme = App.Configuration.DefaultTheme;
            return Entity;
        }

        /// <summary>
        /// Deletes the specified user and all of his snippets and comments
        /// </summary>
        /// <param name="Pk"></param>
        /// <returns></returns>
        public override bool Delete(object Pk)
        {
            string pk = Pk as string;

            if (string.IsNullOrEmpty(pk))
            {
                SetError("No user key provided for deletion");
                return false;
            }
           
            
            // clear all code snippets and comments from this user
            int result = Context.Db.ExecuteNonQuery("delete codesnippets where userId=@Pk;" +
                                                    "delete comments where userId=@Pk",
                    Context.Db.CreateParameter("@Pk", pk));

            if (result == -1)
            {
                SetError(Context.Db.ErrorMessage);
                return false;
            }

            return base.Delete(Pk);                
        }

        /// <summary>
        /// Returns the name fo the the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetNameFromUserId(string userId)
        {
            return Context.Users.Where(usr => usr.Id == userId).Select(usr => usr.Name).SingleOrDefault();
        }

        public void SetUserForEmailValidation(User user = null)
        {
            if (user == null)
                user = Entity;
            if (user == null)
                return;

            user.InActive = true;
            user.Validator = DataUtils.GenerateUniqueId(8);
        }

        protected override bool OnBeforeSave(User entity)
        {           
            if (entity.Email == null)
                entity.Email = string.Empty;

            if (!entity.Password.EndsWith(App.PasswordEncodingPostfix))
                entity.Password = App.EncodePassword(entity.Password, entity.Id);

            entity.Updated = DateTime.UtcNow;

            return base.OnBeforeSave(entity);
        }


        protected override  void OnValidate(User entity)
        {
            base.OnValidate(entity);

            bool isNew = entity.tstamp == null;

            if (string.IsNullOrEmpty(entity.Name))
                ValidationErrors.Add("Name must be provided.", "Name");
            if (string.IsNullOrEmpty(entity.Email))
                ValidationErrors.Add("An email address must be provided.", "Email");

            if (string.IsNullOrEmpty(entity.OpenId))
            {
                if (string.IsNullOrEmpty(entity.Password) || entity.Password.Length < 4)
                    ValidationErrors.Add("Password length must be at least 4 characters.", "Password");

                if (string.IsNullOrEmpty(entity.Email))
                    ValidationErrors.Add("Email address must be provided ", "Email");
            }



            // trying to add new email that already exists?
            if (Entity.tstamp == null && !string.IsNullOrEmpty(entity.Email) )
            {
                if (Context.Users.Where(usr => usr.Email == Entity.Email).Count() > 0)
                    ValidationErrors.Add("Email address/username is already in use.", "Email");
            }
        }

        /// <summary>
        /// Validates a user by username
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public bool ValidateUser(string email, string password)
        {
            SetError();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetError("Empty usernames or passwords are not allowed");
                return false;
            }

            var user = Context.Users
                .FirstOrDefault(usr => usr.Email == email);

            if (user == null)
            {
                SetError("Invalid username or password.");
                return false;
            }

            string encodedPassword = App.EncodePassword(password, user.Id);

            if (encodedPassword != user.Password)
            {
                SetError("Invalid username or password.");
                return false;
            }

            if (user.InActive)
            {
                SetError(
                    "This account is not validated yet. Please check your email for the validation message and click the embedded link to validate your account.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Authenticates a user and if successful returns a user instance
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User ValidateUserAndLoad(string email, string password)
        {
            SetError();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetError("Empty usernames or passwords are not allowed");
                return null;
            }

            User user = LoadBase( usr => usr.Email == email);

            if (user == null)
            {
                SetError("Invalid username or password.");
                return null;
            }

            string encodedPassword = App.EncodePassword(password, user.Id);

            if (encodedPassword != user.Password)
            {
                SetError("Invalid username or password.");
                return null;
            }

            if (user.InActive)
            {
                SetError(
                    "This account is not validated yet. Please check your email for the validation message and click the embedded link to validate your account.");
                return null;
            }

            Entity = user;

            return user;
        }
        
        /// <summary>
        /// Users open ID account. NOTE should only be called if returned from
        /// a successful OpenId validation
        /// </summary>
        /// <param name="providerKey"></param>
        /// <returns></returns>
        public User ValidateUserWithExternalLogin(string providerKey)
        {
            SetError();
            if (string.IsNullOrEmpty(providerKey))
            {
                SetError("Invalid login.");
                return null;
            }

            User user = LoadBase( usr => usr.OpenId == providerKey || usr.OpenIdClaim == providerKey);
            if (user == null)
            {                
                SetError("OpenId Login is not associated with an account.");
                return null;
            }
        
            Entity = user;
            return user;
        }

        public User ValidateEmailAddress(string validator)
        {
            var user = LoadBase( usr => usr.Validator == validator);
            if (user == null)
                throw new ApplicationException("Invalid email validator id.");

            user.InActive = false;
            user.Validator = null;

            if (!Save())
                throw new ApplicationException("Unable to validate email address at this time.");

            return user;
        }

        /// <summary>
        /// Loads a user from the email address
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public User LoadUserByEmail(string email)
        {
            return LoadBase("select * from Users where email={0}", email);
        }

        /// <summary>
        /// Loads a user from the email address.
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public User LoadUserByProviderKey(string providerKey)
        {
            return LoadBase("select * from Users where openIdClaim={0}", providerKey);
        }


        /// <summary>
        /// Returns a list of items
        /// </summary>
        /// <param name="whereClauseLambda"></param>
        /// <returns></returns>
        public List<User> GetUserList(Func<User, bool> whereClauseLambda)
        {
            List<User> userList = null;

            if (whereClauseLambda == null)
                userList = Context.Users.OrderByDescending(usr => usr.Entered).ToList();
            else
            {
                userList = Context.Users.Where(whereClauseLambda)
                                              .OrderByDescending(usr => usr.Entered)
                                              .ToList();
            }

            foreach (User user in userList)
            {
                user.Password = string.Empty;
            }
            return userList;
        }

        /// <summary>
        /// Updates the snippet counts for all users in the application
        /// </summary>
        /// <returns></returns>
        public bool UpdateAllUserSnippetCounts()
        {
            string sql = "update users set snippets = (select count(*) from CodeSnippets where USERID = users.Id)";
            return ExecuteNonQuery(sql) > -1;
        }
    }
}
