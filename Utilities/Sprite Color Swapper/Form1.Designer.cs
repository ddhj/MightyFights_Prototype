namespace Sprite_Color_Swapper
{
	partial class Form1
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
			this._cButBuild = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._cTxtFile = new System.Windows.Forms.TextBox();
			this._cTxtCmf = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this._cTxtProgress = new System.Windows.Forms.TextBox();
			this._cLabelProgress = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _cButBuild
			// 
			this._cButBuild.Location = new System.Drawing.Point(166, 198);
			this._cButBuild.Name = "_cButBuild";
			this._cButBuild.Size = new System.Drawing.Size(75, 23);
			this._cButBuild.TabIndex = 0;
			this._cButBuild.Text = "Build";
			this._cButBuild.UseVisualStyleBackColor = true;
			this._cButBuild.Click += new System.EventHandler(this._cButBuild_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(45, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "File:";
			// 
			// _cTxtFile
			// 
			this._cTxtFile.Location = new System.Drawing.Point(77, 23);
			this._cTxtFile.Name = "_cTxtFile";
			this._cTxtFile.ReadOnly = true;
			this._cTxtFile.Size = new System.Drawing.Size(195, 20);
			this._cTxtFile.TabIndex = 2;
			// 
			// _cTxtCmf
			// 
			this._cTxtCmf.Location = new System.Drawing.Point(77, 63);
			this._cTxtCmf.Name = "_cTxtCmf";
			this._cTxtCmf.ReadOnly = true;
			this._cTxtCmf.Size = new System.Drawing.Size(195, 20);
			this._cTxtCmf.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(22, 66);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(49, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "# of cmf:";
			// 
			// _cTxtProgress
			// 
			this._cTxtProgress.Location = new System.Drawing.Point(77, 230);
			this._cTxtProgress.Name = "_cTxtProgress";
			this._cTxtProgress.ReadOnly = true;
			this._cTxtProgress.Size = new System.Drawing.Size(195, 20);
			this._cTxtProgress.TabIndex = 6;
			this._cTxtProgress.Visible = false;
			// 
			// _cLabelProgress
			// 
			this._cLabelProgress.AutoSize = true;
			this._cLabelProgress.Location = new System.Drawing.Point(22, 233);
			this._cLabelProgress.Name = "_cLabelProgress";
			this._cLabelProgress.Size = new System.Drawing.Size(51, 13);
			this._cLabelProgress.TabIndex = 5;
			this._cLabelProgress.Text = "Progress:";
			this._cLabelProgress.Visible = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this._cTxtProgress);
			this.Controls.Add(this._cLabelProgress);
			this.Controls.Add(this._cTxtCmf);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._cTxtFile);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._cButBuild);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _cButBuild;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _cTxtFile;
		private System.Windows.Forms.TextBox _cTxtCmf;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _cTxtProgress;
		private System.Windows.Forms.Label _cLabelProgress;
	}
}

