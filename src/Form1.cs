using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BottomMost
{
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

   public struct WINDOWRECT
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
    
    

    public partial class Form1 : Form
    {
        public WINDOWRECT RectToWindowRect(RECT rect)
        {
            WINDOWRECT windowRect = new WINDOWRECT();
            windowRect.x = rect.left;
            windowRect.y = rect.top;
            windowRect.width = rect.right - rect.left;
            windowRect.height = rect.bottom - rect.top;
            return windowRect;
        }

        IntPtr holdWindow; // 対象のウィンドウ
        //Array<System.Diagnostics.Process> processList;
        ArrayList processList;
        WINDOWRECT preWindowRect;
        IntPtr preActiveWindow;


        // DLL
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        private const int HWND_BOTTOM = 1;
        private const int HWND_NOTOPMOST = -2;
        private const int HWND_TOP = 0;
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOACTIVATE = 0x10;

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr HWND, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetActiveWindow();

        private bool GetWindowRect(IntPtr HWND,out WINDOWRECT wRect)
        {
            RECT rect;
            bool flag=GetWindowRect(HWND, out rect);
            wRect = RectToWindowRect(rect);
            return flag;
        }

        public Form1()
        {
            InitializeComponent();

            this.processList = new ArrayList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.UpdateWindow();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.TimerClock();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void StopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
        }

        // ここから作成したメソッド

        // 実行中のプロセスリストを更新する 
        private void UpdateWindow()
        {
            // リストボックスを初期化する
            this.listBox1.Items.Clear();

            // 起動しているプロセスのリストを初期化する
            this.processList.Clear();

            // 全てのウィンドウを調べる
            foreach (System.Diagnostics.Process p in
                System.Diagnostics.Process.GetProcesses())
            {
                //メインウィンドウのタイトルがある時だけ列挙する
                if (p.MainWindowTitle.Length != 0)
                {
                    // ウィンドウをリストボックスに追加する
                    this.listBox1.Items.Add(p.MainWindowTitle);

                    // プロセスリストにも追加
                    this.processList.Add((IntPtr)p.MainWindowHandle);
                }
            }
        } // end Update

        // Clock
        private void TimerClock()
        {
            // TO DO: ウィンドウが最小化されていれば表示する
            //ShowWindow(this.holdWindow, 8);

            // ウィンドウの形状の取得
            WINDOWRECT wRect;
            GetWindowRect(this.holdWindow, out wRect);

            // アクティブウィンドウの取得
            IntPtr activeWindow = GetActiveWindow();

            if (wRect.x < -10000)
            {
                // 最小化されているとみなす

                wRect = preWindowRect;
                //SetWindowPos(this.holdWindow, HWND_BOTTOM, wRect.x, wRect.y, wRect.width, wRect.height, 2);
                ShowWindow(this.holdWindow, SW_RESTORE);
                GetWindowRect(this.holdWindow, out wRect);
                activeWindow = GetActiveWindow();
            }
            else
            {
                // ウィンドウの位置が変更されている場合，移動しなおす
                if (wRect.x != preWindowRect.x || wRect.y != preWindowRect.y || wRect.width != preWindowRect.width || wRect.height != preWindowRect.height || preActiveWindow != activeWindow)
                {
                    SetWindowPos(this.holdWindow, HWND_BOTTOM, wRect.x, wRect.y, wRect.width, wRect.height, SWP_NOACTIVATE);
                }

            }
            textBox1.Text = wRect.x.ToString() + "," + wRect.y.ToString() + "," + wRect.width.ToString() + "," + wRect.height.ToString();

            // 保存情報の登録
            preWindowRect = wRect;
            preActiveWindow = activeWindow;
        } // end TimerClock

        // 固定開始
        private void Start()
        {
            // ウィンドウが選択されていなければ実行しない
            if (this.listBox1.SelectedItem == null)
            {
                return;
            }

            // 固定するウィンドウのハンドルを取得
            this.holdWindow = (IntPtr)processList[listBox1.SelectedIndex];
            
            // タイマー開始
            this.timer1.Start();

            // 自身のウィンドウを非表示にする
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }
        
    }
}
