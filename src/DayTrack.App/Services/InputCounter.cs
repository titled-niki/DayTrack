using System.Diagnostics; using System.Runtime.InteropServices;
namespace DayTrack.Services;
/// <summary>Counts totals only. It never reads key codes, typed text, clipboard, or mouse coordinates.</summary>
public sealed class InputCounter:IDisposable
{
 const int WH_KEYBOARD_LL=13,WH_MOUSE_LL=14,WM_KEYDOWN=0x0100,WM_SYSKEYDOWN=0x0104,WM_LBUTTONDOWN=0x0201,WM_RBUTTONDOWN=0x0204,WM_MBUTTONDOWN=0x0207,WM_XBUTTONDOWN=0x020B;
 delegate IntPtr HookProc(int nCode,IntPtr wParam,IntPtr lParam); readonly HookProc _kp,_mp; IntPtr _kh,_mh; long _keys,_clicks;
 public long KeyPresses=>Interlocked.Read(ref _keys); public long MouseClicks=>Interlocked.Read(ref _clicks);
 public InputCounter(){_kp=Keyboard;_mp=Mouse;}
 public void Start(){if(_kh!=IntPtr.Zero||_mh!=IntPtr.Zero)return; using var p=Process.GetCurrentProcess(); using var m=p.MainModule; var hm=GetModuleHandle(m?.ModuleName); _kh=SetWindowsHookEx(WH_KEYBOARD_LL,_kp,hm,0); _mh=SetWindowsHookEx(WH_MOUSE_LL,_mp,hm,0);}
 IntPtr Keyboard(int c,IntPtr w,IntPtr l){if(c>=0&&(w.ToInt32()==WM_KEYDOWN||w.ToInt32()==WM_SYSKEYDOWN)) Interlocked.Increment(ref _keys); return CallNextHookEx(_kh,c,w,l);}
 IntPtr Mouse(int c,IntPtr w,IntPtr l){if(c>=0&&w.ToInt32() is WM_LBUTTONDOWN or WM_RBUTTONDOWN or WM_MBUTTONDOWN or WM_XBUTTONDOWN) Interlocked.Increment(ref _clicks); return CallNextHookEx(_mh,c,w,l);}
 public void Dispose(){if(_kh!=IntPtr.Zero)UnhookWindowsHookEx(_kh);if(_mh!=IntPtr.Zero)UnhookWindowsHookEx(_mh);_kh=_mh=IntPtr.Zero;}
 [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SetWindowsHookEx(int id,HookProc p,IntPtr m,uint t); [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr h); [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr h,int c,IntPtr w,IntPtr l); [DllImport("kernel32.dll",CharSet=CharSet.Auto)] static extern IntPtr GetModuleHandle(string? n);
}
