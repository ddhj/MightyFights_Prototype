using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MightyFights_Prototype
{
	public partial class CapCompTempEditor : Form
	{
		public CapCompTempEditor()
		{
			InitializeComponent();
		}

		private void _cTemp1CB_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void _cTemp4CB_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void _cTemp2CB_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void _cTemp3CB_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void _cOk_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void CapCompTempEditor_Load(object sender, EventArgs e)
		{
			foreach(Company cCompany in DataStore.cInstance.cLSteward.caCompanies) { 
				_cCompanyCB.Items.Add(cCompany);
			}

		}

		void LoadCompany(Company cCompany) 
		{

		}

		private void _cCompanyImage_Click(object sender, EventArgs e)
		{

		}

		private void _cTemp1Count_ValueChanged(object sender, EventArgs e)
		{

		}

		private void _cTemp2Count_ValueChanged(object sender, EventArgs e)
		{

		}

		private void _cTemp3Count_ValueChanged(object sender, EventArgs e)
		{

		}

		private void _cTemp4Count_ValueChanged(object sender, EventArgs e)
		{

		}
	}
}
