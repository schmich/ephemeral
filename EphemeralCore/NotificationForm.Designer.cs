namespace Ephemeral
{
    partial class NotificationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._container = new System.Windows.Forms.Panel();
            this._messageLabel = new Ephemeral.SmoothLabel();
            this._container.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this._container.AutoSize = true;
            this._container.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._container.Controls.Add(this._messageLabel);
            this._container.Dock = System.Windows.Forms.DockStyle.Fill;
            this._container.Location = new System.Drawing.Point(0, 0);
            this._container.Name = "panel1";
            this._container.Size = new System.Drawing.Size(424, 218);
            this._container.TabIndex = 0;
            // 
            // smoothLabel1
            // 
            this._messageLabel.AutoSize = true;
            this._messageLabel.Centered = false;
            this._messageLabel.ForeColor = System.Drawing.Color.White;
            this._messageLabel.Location = new System.Drawing.Point(176, 81);
            this._messageLabel.Name = "smoothLabel1";
            this._messageLabel.Size = new System.Drawing.Size(65, 13);
            this._messageLabel.TabIndex = 0;
            this._messageLabel.Text = "Hello World!";
            // 
            // NotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(424, 218);
            this.ControlBox = false;
            this.Controls.Add(this._container);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationForm";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this._container.ResumeLayout(false);
            this._container.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _container;
        private SmoothLabel _messageLabel;

    }
}