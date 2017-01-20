using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace lingvo.crfsuite
{
    /// <summary>
    /// Загрузчик шаблонов
    /// </summary>
    public static class CRFTemplateFileLoader
    {
        #region [.private field's.]
        // Названия групп внутри регулярных выражений
        private const string Templates = "Templates";
        private const string Template  = "Template";
        private const string Fields    = "Fields";

        /// Регулярное выражение для выделения шаблонов
        private static readonly Regex _TemplatesRegex = new Regex("templates\\s*=\\s*\\((?<" + Templates + ">(\\s*.+)*)\\s*\\)", RegexOptions.IgnoreCase);
        
        /// регулярное выражение для выделения одного шаблона
        private static readonly Regex _TemplateRegex = new Regex("\\((?<" + Template + ">[^(]*)\\),", RegexOptions.IgnoreCase);
        
        /// Регулярное выражение для выделения названий столбцов
        private static readonly Regex _FieldsRegex = new Regex("fields\\s*=\\s*'(?<" + Fields + ">[^']*)'", RegexOptions.IgnoreCase);
        #endregion

        ///Загрузить файл шаблона
		///@param filePath Путь к файлу шаблона
		///@return файл шаблона
        public static CRFTemplateFile Load( string filePath )
		{
			using ( var sr = new StreamReader( filePath ) )
            {
			    string fileString = sr.ReadToEnd();

			    char[]                  columnNames           = ExtractColumnNames( fileString );
                Dictionary< char, int > columnIndexDictionary = CreateColumnIndexDictionary( columnNames );
			    CRFNgram[]              attributeTemplates    = ExtractAttributeTemplates( fileString, columnIndexDictionary );

			    return (new CRFTemplateFile( columnNames, attributeTemplates ));
            }
		}
        public static CRFTemplateFile Load( string filePath, char[] allowedColumnNames )
        {
            var crfTemplateFile = Load( filePath );

            if ( allowedColumnNames != null && allowedColumnNames.Length != 0 )
            {
                var hs = new HashSet< char >( allowedColumnNames );
                foreach ( var columnName in crfTemplateFile.ColumnNames )
                {
                    if ( !hs.Contains( columnName ) )
                    {
                        throw (new InvalidDataException( "Invalid column-name: '" + columnName + 
                            "', allowed only '" + string.Join( ",", allowedColumnNames ) + "'" ));
                    }
                }
            }
            return (crfTemplateFile);
        }

        // Извлечь шаблоны аттрибутов
        // @param fileString - Содержимое файла-шаблона
        // @return - Шаблоны аттрибутов
        private static CRFNgram[] ExtractAttributeTemplates( string fileString, Dictionary< char, int > columnIndexDictionary )
        {
            var attributeTemplateStrings = ExtractAttributeTemplateStrings( fileString );
            var split_chars = new[] { ',' };
            var attributeTemplates = new List< CRFNgram >( attributeTemplateStrings.Length );
            foreach ( var str in attributeTemplateStrings )
            {
                MatchCollection matchCollection = _TemplateRegex.Matches( str );
                if ( matchCollection.Count == 0 )
                    continue;

                var attributeTemplate = new List< CRFAttribute >( matchCollection.Count );
                foreach ( Match currentMatch in matchCollection )
                {
                    var oneTemplate = currentMatch.Groups[ Template ].Value;
                    var pair = oneTemplate.Split( split_chars );

                    var attributeName = ParseAttributeName( pair[ 0 ] );
                    if ( attributeName.Length != 1 )
                    {
                        throw (new InvalidDataException( "attribute-name is not valid, must be one-char: '" + attributeName + '\'' ));
                    }
                    var attributeNameChar = attributeName[ 0 ]; //char.ToUpperInvariant( attributeName[ 0 ] );
                    var position      = Int32.Parse( pair[ 1 ] );
                    var columnIndex   = columnIndexDictionary[ attributeNameChar ];                    

                    attributeTemplate.Add( new CRFAttribute( attributeNameChar, position, columnIndex ) );
                }
                attributeTemplates.Add( new CRFNgram( attributeTemplate.ToArray() ) );
            }
            return (attributeTemplates.ToArray());
        }

        // Извлечь название аттрибута
        // @param attrStr - Строка, содержащая название аттрибута
        // @return - Название аттрибута
        private static string ParseAttributeName( string attrStr )
        {
            int startIndex = attrStr.IndexOf( '\'' ) + 1;
            int endIndex   = attrStr.IndexOf( '\'', startIndex);
            return (attrStr.Substring( startIndex, endIndex - startIndex ));
        }

        // Извлечь названия столбцов
        // @param fileString - Содержимое файла-шаблона
        // @return - Названия столбцов
        private static char[] ExtractColumnNames( string fileString )
        {
            Match match = _FieldsRegex.Match( fileString );
            var columnNames = match.Groups[ Fields ].Value.Split( ' ', '\t', '\n' );
            var columnNameChars = new char[ columnNames.Length ];
            for ( int i = 0; i < columnNames.Length; i++ )
            {
                var columnName = columnNames[ i ];
                if ( columnName.Length != 1 )
                {
                    throw (new InvalidDataException( "column-name is not valid, must be one-char: '" + columnName + '\'' ));
                }
                columnNameChars[ i ] = columnName[ 0 ];
            }
            return (columnNameChars);
        }

        // Извлечь строки, соответствующие шаблонам аттрибутов
        // @param fileString - Содержимое файла-шаблона
        // @return - Строки, соответствующие шаблонам аттрибутов
        private static string[] ExtractAttributeTemplateStrings( string fileString )
        {
            Match templatesMatch = _TemplatesRegex.Match( fileString );
            string templates = templatesMatch.Groups[ Templates ].Value;

            templates = Regex.Replace( templates, "\\s*\\(\\s*\\(\\s*", "(" );
            templates = Regex.Replace( templates, ",\\s*\\)\\s*,", ",\n" );

            return (templates.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ));
        }

		/// <summary>
        /// Проинициализировать словарь индексов аттрибутов
		/// </summary>
        private static Dictionary< char, int > CreateColumnIndexDictionary( char[] columnNames )
		{
            var dict = new Dictionary< char, int >();
			var index = 0;
            foreach ( var columnName in columnNames )
			{
				dict.Add( columnName, index );
				index++;
			}
            return (dict);
		}
    }
}
