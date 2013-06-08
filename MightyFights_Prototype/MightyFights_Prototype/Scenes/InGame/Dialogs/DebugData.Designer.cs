namespace MightyFights_Prototype
{
	partial class DebugData
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
			if(disposing && (components != null)) {
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this._cLifeBar = new System.Windows.Forms.CheckBox();
			this._cDmgNum = new System.Windows.Forms.CheckBox();
			this._cHealSpots = new System.Windows.Forms.CheckBox();
			this._cBattleZone = new System.Windows.Forms.CheckBox();
			this._cSlowMo = new System.Windows.Forms.CheckBox();
			this._cMusic = new System.Windows.Forms.CheckBox();
			this._cShowBg = new System.Windows.Forms.CheckBox();
			this._cOK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.flowLayoutPanel1);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(518, 77);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Toggles";
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this._cLifeBar);
			this.flowLayoutPanel1.Controls.Add(this._cDmgNum);
			this.flowLayoutPanel1.Controls.Add(this._cHealSpots);
			this.flowLayoutPanel1.Controls.Add(this._cBattleZone);
			this.flowLayoutPanel1.Controls.Add(this._cSlowMo);
			this.flowLayoutPanel1.Controls.Add(this._cMusic);
			this.flowLayoutPanel1.Controls.Add(this._cShowBg);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(512, 58);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// _cLifeBar
			// 
			this._cLifeBar.AutoSize = true;
			this._cLifeBar.Location = new System.Drawing.Point(3, 3);
			this._cLifeBar.Name = "_cLifeBar";
			this._cLifeBar.Size = new System.Drawing.Size(67, 17);
			this._cLifeBar.TabIndex = 0;
			this._cLifeBar.Text = "Life Bars";
			this._cLifeBar.UseVisualStyleBackColor = true;
			this._cLifeBar.CheckedChanged += new System.EventHandler(this._cLifeBar_CheckedChanged);
			// 
			// _cDmgNum
			// 
			this._cDmgNum.AutoSize = true;
			this._cDmgNum.Location = new System.Drawing.Point(76, 3);
			this._cDmgNum.Name = "_cDmgNum";
			this._cDmgNum.Size = new System.Drawing.Size(83, 17);
			this._cDmgNum.TabIndex = 1;
			this._cDmgNum.Text = "Damage #\'s";
			this._cDmgNum.UseVisualStyleBackColor = true;
			this._cDmgNum.CheckedChanged += new System.EventHandler(this._cDmgNum_CheckedChanged);
			// 
			// _cHealSpots
			// 
			this._cHealSpots.AutoSize = true;
			this._cHealSpots.Location = new System.Drawing.Point(165, 3);
			this._cHealSpots.Name = "_cHealSpots";
			this._cHealSpots.Size = new System.Drawing.Size(78, 17);
			this._cHealSpots.TabIndex = 2;
			this._cHealSpots.Text = "Heal Spots";
			this._cHealSpots.UseVisualStyleBackColor = true;
			this._cHealSpots.CheckedChanged += new System.EventHandler(this._cHealSpots_CheckedChanged);
			// 
			// _cBattleZone
			// 
			this._cBattleZone.AutoSize = true;
			this._cBattleZone.Location = new System.Drawing.Point(249, 3);
			this._cBattleZone.Name = "_cBattleZone";
			this._cBattleZone.Size = new System.Drawing.Size(86, 17);
			this._cBattleZone.TabIndex = 3;
			this._cBattleZone.Text = "Battle Zones";
			this._cBattleZone.UseVisualStyleBackColor = true;
			this._cBattleZone.CheckedChanged += new System.EventHandler(this._cBattleZone_CheckedChanged);
			// 
			// _cSlowMo
			// 
			this._cSlowMo.AutoSize = true;
			this._cSlowMo.Location = new System.Drawing.Point(341, 3);
			this._cSlowMo.Name = "_cSlowMo";
			this._cSlowMo.Size = new System.Drawing.Size(81, 17);
			this._cSlowMo.TabIndex = 4;
			this._cSlowMo.Text = "SlowMotion";
			this._cSlowMo.UseVisualStyleBackColor = true;
			this._cSlowMo.CheckedChanged += new System.EventHandler(this._cSlowMo_CheckedChanged);
			// 
			// _cMusic
			// 
			this._cMusic.AutoSize = true;
			this._cMusic.Location = new System.Drawing.Point(428, 3);
			this._cMusic.Name = "_cMusic";
			this._cMusic.Size = new System.Drawing.Size(77, 17);
			this._cMusic.TabIndex = 5;
			this._cMusic.Text = "Play Music";
			this._cMusic.UseVisualStyleBackColor = true;
			this._cMusic.CheckedChanged += new System.EventHandler(this._cMusic_CheckedChanged);
			// 
			// _cShowBg
			// 
			this._cShowBg.AutoSize = true;
			this._cShowBg.Location = new System.Drawing.Point(3, 26);
			this._cShowBg.Name = "_cShowBg";
			this._cShowBg.Size = new System.Drawing.Size(71, 17);
			this._cShowBg.TabIndex = 6;
			this._cShowBg.Text = "Show BG";
			this._cShowBg.UseVisualStyleBackColor = true;
			this._cShowBg.CheckedChanged += new System.EventHandler(this._cShowBg_CheckedChanged);
			// 
			// _cOK
			// 
			this._cOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cOK.Location = new System.Drawing.Point(431, 265);
			this._cOK.Name = "_cOK";
			this._cOK.Size = new System.Drawing.Size(75, 23);
			this._cOK.TabIndex = 1;
			this._cOK.Text = "OK";
			this._cOK.UseVisualStyleBackColor = true;
			this._cOK.Click += new System.EventHandler(this._cOK_Click);
			// 
			// DebugData
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(518, 300);
			this.ControlBox = false;
			this.Controls.Add(this._cOK);
			this.Controls.Add(this.groupBox1);
			this.Name = "DebugData";
			this.Text = "Debug Data";
			this.groupBox1.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.CheckBox _cLifeBar;
		private System.Windows.Forms.CheckBox _cDmgNum;
		private System.Windows.Forms.CheckBox _cHealSpots;
		private System.Windows.Forms.CheckBox _cBattleZone;
		private System.Windows.Forms.CheckBox _cSlowMo;
		private System.Windows.Forms.CheckBox _cMusic;
		private System.Windows.Forms.CheckBox _cShowBg;
		private System.Windows.Forms.Button _cOK;
	}
}