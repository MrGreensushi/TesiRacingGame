<# :
@echo off
setlocal
set "POWERSHELL_BAT_ARGS=%*"
if defined POWERSHELL_BAT_ARGS set "POWERSHELL_BAT_ARGS=%POWERSHELL_BAT_ARGS:"=\"%"
endlocal & powershell -NoLogo -NoProfile -Command "$_ = $input; Invoke-Expression $( '$input = $_; $_ = \"\"; $args = @( &{ $args } %POWERSHELL_BAT_ARGS% );' + [String]::Join( [char]10, $( Get-Content \"%~f0\" ) ) )"
goto :EOF
#>

Add-Type @"
  using System;
  using System.Runtime.InteropServices;

  public class Win32 {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
  }

  public struct RECT
  {
    public int Left;        // x position of upper-left corner
    public int Top;         // y position of upper-left corner
    public int Right;       // x position of lower-right corner
    public int Bottom;      // y position of lower-right corner
  }
"@

$progname = $args[2]
$progargs = $args[3..($args.Count-1)]
#Write-Host "progargs=$progargs"
$rcWindow = New-Object RECT
$MyProcess = Start-Process -FilePath $progname -ArgumentList $progargs -PassThru

While ($MyProcess.MainWindowHandle -eq 0) {
Start-Sleep -Seconds 1
}

$h = $MyProcess.MainWindowHandle

# Set GetWindowRect output = to $ret so that it doesn't print 'True' to the terminal
$ret = [Win32]::GetWindowRect($h,[ref]$rcWindow)

$win_width = $rcWindow.Right - $rcWindow.Left
$win_height = $rcWindow.Bottom - $rcWindow.Top
$screen_x=$args[0]
$screen_y=$args[1]

# Set MoveWindow output = to $ret so that it doesn't print 'True' to the terminal
$ret = [Win32]::MoveWindow($h, $screen_x, $screen_y, $win_width, $win_height, $true )



