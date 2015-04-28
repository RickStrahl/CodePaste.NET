#region Copyright © 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */ 
#endregion

namespace Manoli.Utils.CSharpFormat
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Generates color-coded HTML 4.01 from C# source code.
	/// </summary>
	public class CPlusPlusFormat : CLikeFormat
	{                
		/// <summary>
		/// The list of C# keywords.
		/// </summary>
		protected override string Keywords 
		{
			get 
			{
                return
@"ATOM BOOL BOOLEAN BYTE CHAR COLORREF CRITICAL_SECTION DWORD DWORDLONG DWORD_PTR
DWORD32 DWORD64 FLOAT HACCEL HALF_PTR HANDLE HBITMAP HBRUSH
HCOLORSPACE HCONV HCONVLIST HCURSOR HDC HDDEDATA HDESK HDROP HDWP
HENHMETAFILE HFILE HFONT HGDIOBJ HGLOBAL HHOOK HICON HINSTANCE HKEY
HKL HLOCAL HMENU HMETAFILE HMODULE HMONITOR HPALETTE HPEN HRESULT
HRGN HRSRC HSZ HWINSTA HWND INT INT_PTR INT32 INT64 LANGID LCID LCTYPE
LGRPID LONG LONGLONG LONG_PTR LONG32 LONG64 LPARAM LPBOOL LPBYTE LPCOLORREF
LPCSTR LPCTSTR LPCVOID LPCWSTR LPDWORD LPHANDLE LPINT LPLONG LPSTR LPTSTR
LPVOID LPWORD LPWSTR LRESULT PBOOL PBOOLEAN PBYTE PCHAR PCSTR PCTSTR PCWSTR
PDWORDLONG PDWORD_PTR PDWORD32 PDWORD64 PFLOAT PHALF_PTR PHANDLE PHKEY PINT
PINT_PTR PINT32 PINT64 PLCID PLONG PLONGLONG PLONG_PTR PLONG32 PLONG64 POINTER_32  
POINTER_64 PSHORT PSIZE_T PSSIZE_T PSTR PTBYTE PTCHAR PTSTR PUCHAR PUHALF_PTR  
PUINT PUINT_PTR PUINT32 PUINT64 PULONG PULONGLONG PULONG_PTR PULONG32 PULONG64  
PUSHORT PVOID PWCHAR PWORD PWSTR SC_HANDLE SC_LOCK SERVICE_STATUS_HANDLE SHORT   
SIZE_T SSIZE_T TBYTE TCHAR UCHAR UHALF_PTR UINT UINT_PTR UINT32 UINT64 ULONG  
ULONGLONG ULONG_PTR ULONG32 ULONG64 USHORT USN VOID WCHAR WINAPI WORD WPARAM WPARAM WPARAM  
char bool short int __int32 __int64 __int8 __int16 long float double __wchar_t  
clock_t _complex _dev_t _diskfree_t div_t ldiv_t _exception _EXCEPTION_POINTERS  
FILE _finddata_t _finddatai64_t _wfinddata_t _wfinddatai64_t __finddata64_t  
__wfinddata64_t _FPIEEE_RECORD fpos_t _HEAPINFO _HFILE lconv intptr_t  
jmp_buf mbstate_t _off_t _onexit_t _PNH ptrdiff_t _purecall_handler  
sig_atomic_t size_t _stat __stat64 _stati64 terminate_function  
time_t __time64_t _timeb __timeb64 tm uintptr_t _utimbuf  
va_list wchar_t wctrans_t wctype_t wint_t signed
break case catch class const __finally __exception __try
const_cast continue private public protected __declspec
default delete deprecated dllexport dllimport do dynamic_cast
else enum explicit extern if for friend goto inline
mutable naked namespace new noinline noreturn nothrow
register reinterpret_cast return selectany
sizeof static static_cast struct switch template this
thread throw true false try typedef typeid typename union
using uuid virtual void volatile whcar_t while";
			}
		}

		/// <summary>
		/// The list of C++ preprocessors.
		/// </summary>
		protected override string Preprocessors
		{
			get 
			{ 
				return "#define #error #include #elif #if #line #else #ifdef #pragma #endif #ifndef #undef";
			}
		}
	}  
}

