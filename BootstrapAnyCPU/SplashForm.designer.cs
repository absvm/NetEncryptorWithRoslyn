namespace Bootstrap
{
    partial class SplashForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

 
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._fadeOutTimer = new System.Windows.Forms.Timer(this.components);
            this._fadeInTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // _fadeOutTimer
            // 
            this._fadeOutTimer.Interval = 50;
            this._fadeOutTimer.Tick += new System.EventHandler(this._fadeOutTimer_Tick);
            // 
            // _fadeInTimer
            // 
            this._fadeInTimer.Interval = 50;
            this._fadeInTimer.Tick += new System.EventHandler(this._fadeInTimer_Tick);
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Bootstrap.Properties.Resources.Splash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(306, 306);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Splash";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer _fadeOutTimer;
        private System.Windows.Forms.Timer _fadeInTimer;
    }
}