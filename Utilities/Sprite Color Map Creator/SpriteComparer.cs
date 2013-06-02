// system includes
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Sprite_Color_Map_Creator	{
	class SpriteComparer	{
	// data
		string	_sBaseFile,
				_sBaseDirectory;

		Bitmap	_cBaseImage;

	// constructor
		public SpriteComparer( string sFile )
		{
			_sBaseDirectory = Path.GetDirectoryName( sFile );
			_sBaseFile = Path.GetFileName( sFile );

			_cBaseImage = (Bitmap)Bitmap.FromFile( sFile );
		}

	// functions
		public void Go( )
		{
			string		sFilename;
			string[]	saFiles = Directory.GetFiles( _sBaseDirectory, "*.png" );
			Bitmap		cBitmap;

			foreach( string sFile in saFiles )
			{
				sFilename = Path.GetFileName( sFile );
				if( sFilename != _sBaseFile )
				{
					Console.WriteLine( string.Format( "Comparing {0} now", sFilename ));
					cBitmap = (Bitmap)Bitmap.FromFile( sFile );
					BuildMapFile( cBitmap, sFilename );
				}
			}
		}

		void BuildMapFile( Bitmap cBitmap, string sFile )
		{
			Dictionary<Color,Color>		cMap = new Dictionary<Color,Color>( );
			List<string>	saMappingFile = new List<string>( );
			Color	tBase,
					tOther;

			for( int iY = 0; iY < _cBaseImage.Size.Height; ++iY )
				for( int iX = 0; iX < _cBaseImage.Size.Width; ++iX )
				{
					tBase = _cBaseImage.GetPixel( iX, iY );
					tOther = cBitmap.GetPixel( iX, iY );
					if( tBase != tOther )
					{
						if( tBase.A < 5 || tOther.A < 5 )
							continue;

						if( !cMap.ContainsKey( tBase ))
							cMap.Add( tBase, tOther );
					}
				}

			foreach( KeyValuePair<Color, Color> tPair in cMap )
				saMappingFile.Add( string.Format( "{0},{1}", tPair.Key, tPair.Value ));

			File.WriteAllLines( Path.Combine( _sBaseDirectory, string.Format( "{0}.cmf", sFile.Split( '.' )[0] )), saMappingFile.ToArray( ));
		}
	}
}
