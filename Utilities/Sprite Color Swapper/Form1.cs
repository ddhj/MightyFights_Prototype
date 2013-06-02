// system includes
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Sprite_Color_Swapper	{
	public partial class Form1 : Form	{
	// Data
		string		_sBaseFile,
					_sBaseDirectory;
		string[]	_saMapFiles;

		Bitmap		_cBaseImage;

		Dictionary<string,Dictionary<Color,Color>>	_cMaps = new Dictionary<string,Dictionary<Color,Color>>( );
	// Constructors
		public Form1( string sFile )
		{
			InitializeComponent();

			_sBaseDirectory = Path.GetDirectoryName( sFile );
			_sBaseFile = Path.GetFileName( sFile );

			_cBaseImage = (Bitmap)Bitmap.FromFile( sFile );
			_saMapFiles = Directory.GetFiles( _sBaseDirectory, "*.cmf" );

			_cTxtFile.Text = _sBaseFile;
			_cTxtCmf.Text = _saMapFiles.Length.ToString( );

			{
				string[]	saFile,
							saMapping,
							saOrigColors,
							saNewColors;
				Dictionary<Color,Color>	cMap;
				foreach( string sMap in _saMapFiles )
				{
					cMap = new Dictionary<Color,Color>( );
					_cMaps.Add( Path.GetFileNameWithoutExtension( sMap ), cMap );

					saFile = File.ReadAllLines( sMap );

					foreach( string sLine in saFile )
					{
						saMapping = sLine.Split( new string[]{ "Color ", "[", "]" }, StringSplitOptions.RemoveEmptyEntries );

						saOrigColors = saMapping[0].Split( '=', ',' );
						saNewColors = saMapping[2].Split( '=', ',' );
						cMap.Add( Color.FromArgb( Convert.ToInt32( saOrigColors[1]), Convert.ToInt32( saOrigColors[3]), Convert.ToInt32( saOrigColors[5]), Convert.ToInt32( saOrigColors[7])),
									Color.FromArgb( Convert.ToInt32( saNewColors[1]), Convert.ToInt32( saNewColors[3]), Convert.ToInt32( saNewColors[5]), Convert.ToInt32( saNewColors[7])));
					}
				}
			}
		}

	// Functions

	// Events
		void _cButBuild_Click(object sender, EventArgs e)
		{
			Bitmap	cNewImage;
			Color	tBase,
					tNew;

			Cursor.Current = Cursors.WaitCursor;
			_cLabelProgress.Visible = _cTxtProgress.Visible = true;
			foreach( KeyValuePair<string,Dictionary<Color,Color>> tPair in _cMaps )
			{
				_cTxtProgress.Text = tPair.Key;
				Refresh( );

				cNewImage = (Bitmap)_cBaseImage.Clone( );
				for( int iY = 0; iY < _cBaseImage.Size.Height; ++iY )
					for( int iX = 0; iX < _cBaseImage.Size.Width; ++iX )
					{
						tBase = cNewImage.GetPixel( iX, iY );
						if( tPair.Value.TryGetValue( tBase, out tNew ))
						{
							cNewImage.SetPixel( iX, iY, tNew );
						}
					}
				cNewImage.Save( Path.Combine( _sBaseDirectory, string.Format( "{0}.png", tPair.Key )), System.Drawing.Imaging.ImageFormat.Png );
			}

			Cursor.Current = Cursors.Default;
			this.Close( );
		}
	}
}
