namespace Bomberman
{
    partial class ServerControl
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.StatusText = new System.Windows.Forms.TextBox();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.RestartButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(11, 15);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(77, 13);
            this.StatusLabel.TabIndex = 5;
            this.StatusLabel.Text = "Server Status :";
            // 
            // StatusText
            // 
            this.StatusText.Location = new System.Drawing.Point(94, 12);
            this.StatusText.Name = "StatusText";
            this.StatusText.ReadOnly = true;
            this.StatusText.Size = new System.Drawing.Size(253, 20);
            this.StatusText.TabIndex = 6;
            // 
            // RefreshTimer
            // 
            this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // RestartButton
            // 
            this.RestartButton.Enabled = false;
            this.RestartButton.Location = new System.Drawing.Point(353, 10);
            this.RestartButton.Name = "RestartButton";
            this.RestartButton.Size = new System.Drawing.Size(60, 23);
            this.RestartButton.TabIndex = 7;
            this.RestartButton.Text = "Start";
            this.RestartButton.UseVisualStyleBackColor = true;
            this.RestartButton.Click += new System.EventHandler(this.RestartButton_Click);
            // 
            // ServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 42);
            this.Controls.Add(this.RestartButton);
            this.Controls.Add(this.StatusText);
            this.Controls.Add(this.StatusLabel);
            this.MaximumSize = new System.Drawing.Size(600, 300);
            this.Name = "ServerControl";
            this.Text = "Bomberman Server";
            this.Load += new System.EventHandler(this.ServerControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.TextBox StatusText;
        private System.Windows.Forms.Timer RefreshTimer;
        private System.Windows.Forms.Button RestartButton;
    }
}

