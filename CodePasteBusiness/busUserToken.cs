using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using Westwind.BusinessFramework.LinqToSql;
using Westwind.Utilities;
//using System.Data.SqlClient;


namespace CodePasteBusiness
{
    
    /// <summary>
    /// Class that creates and checks for API session tokens
    /// </summary>
    public class busUserToken :  BusinessObjectLinq<UserToken,CodePasteDataContext>
    {
        
        /// <summary>
        /// The timeout for the token in minutes
        /// </summary>
        public int TokenTimeout
        {
          get { return _TokenTimeout; }
          set { _TokenTimeout = value; }
        }
        private int _TokenTimeout = 30;


        private int CreateCounter = 0;

        /// <summary>
        /// Create a new token and write into database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string CreateToken(User user)
        {
            return this.CreateToken(user.Id);
        }

        /// <summary>
        /// Updates a token with sliding expiration
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool UpdateTokenExpiration(string token)
        {
            int count = 
                this.ExecuteNonQuery("update " + this.TableInfo.Tablename + " set Expires=@Expires where token=@Token",
                    this.Context.Db.CreateParameter("@Expires",DateTime.Now.AddMinutes(this.TokenTimeout)),
                    this.Context.Db.CreateParameter("@Token",token) );

            if (count < 0)
                return false;

            return true;
        }

        
        /// <summary>
        /// creates a new token and returns the new token id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string CreateToken(string userId)
        {
            string id = this.CreateTokenString();

            UserToken token = this.NewEntity();
            token.UserId = userId;
            token.Token = id;
            token.Entered = DateTime.Now;
            token.Expires = DateTime.Now.AddMinutes(this.TokenTimeout);

            if (!this.Save())
                return null;
            

            return id;
        }           

        /// <summary>
        /// Retrieves a user object from a token
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public User GetUserFromToken(string userToken)
        {
            busUser User = new busUser();

            User userEntity = 
                (from user in User.Context.Users
                     join tk in User.Context.UserTokens 
                     on user.Id equals tk.UserId
                where tk.Token == userToken && 
                      tk.Expires.CompareTo(DateTime.Now) > -1
                select user).FirstOrDefault();

            return userEntity;
        }

        /// <summary>
        /// Returns a token from a given user
        /// </summary>
        /// <param name="userPk"></param>
        /// <returns></returns>
        public string GetTokenFromUser(string userId)
        {
            string token = 
             (from tk in this.Context.UserTokens
             where tk.UserId == userId &&
                 tk.Expires.CompareTo(DateTime.Now) > -1
             select tk.Token).FirstOrDefault();

            return token;
        }

        /// <summary>
        /// Retrieves a token for a user or creates a new token if it doesn't exist
        /// </summary>
        /// <param name="userPk"></param>
        /// <returns></returns>
        public string GetOrCreateToken(string userId)
        {
            string token = this.GetTokenFromUser(userId);
            if (string.IsNullOrEmpty(token))
                token = this.CreateToken(userId);
            else
                this.UpdateTokenExpiration(token); // sliding expiration

            return token;
        }

        /// <summary>
        /// Checks to see whether a token is valid
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool ValidateToken(string userToken)
        {
            int count = 
                (from token in Context.UserTokens 
                where token.Token == userToken &&
                      token.Expires.CompareTo(DateTime.Now) < 1
                select token.Token).Count();

            return count > 0;
        }


        /// <summary>
        /// Deletes an individual token
        /// </summary>
        /// <param name="token"></param>
        public void DeleteToken(string token)
        {
            this.ExecuteNonQuery("delete from " + this.TableInfo.Tablename + " where token = @token",
                                         this.Context.Db.CreateParameter("@token", token));
        }

        /// <summary>
        /// Deletes timeout tokens
        /// </summary>
        /// <returns></returns>
        public int DeleteExpiredTokens()
        {
            return this.ExecuteNonQuery("delete from " + this.TableInfo.Tablename + " where expires > @expires",                                
                                 this.Context.Db.CreateParameter("@expires",DateTime.Now));
        }

        /// <summary>
        /// Creates an actual token
        /// </summary>
        /// <returns></returns>
        protected string CreateTokenString()
        {
            return DataUtils.GenerateUniqueId();
                                    
        }



    }
}
