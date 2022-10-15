using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Загрузить файл шаблона
        /// </summary>
        /// <param name="filePath">Путь к файлу шаблона</param>
        /// <returns>файл шаблона</returns>
        public static CRFTemplateFile Load( string filePath )
		{
			using ( var sr = new StreamReader( filePath ) )
            {
			    var text = sr.ReadToEnd();

			    var columnNames           = ExtractColumnNames( text );
                var columnIndexDictionary = CreateColumnIndexDictionary( columnNames );
			    var attributeTemplates    = ExtractAttributeTemplates( text, columnIndexDictionary );

			    return (new CRFTemplateFile( columnNames, attributeTemplates ));
            }
		}
        public static CRFTemplateFile Load( string filePath, char[] allowedColumnNames )
        {
            var crfTemplateFile = Load( filePath );

            if ( (allowedColumnNames != null) && (allowedColumnNames.Length != 0) )
            {
                var hs = new HashSet< char >( allowedColumnNames );
                foreach ( var columnName in crfTemplateFile.ColumnNames )
                {
                    if ( !hs.Contains( columnName ) )
                    {
                        throw (new InvalidDataException( $"Invalid column-name: '{columnName}', allowed only '{string.Join( ",", allowedColumnNames )}'" ));
                    }
                }
            }
            return (crfTemplateFile);
        }

        /// <summary>
        /// Извлечь шаблоны аттрибутов
        /// </summary>
        /// <param name="text">Содержимое файла-шаблона</param>
        /// <returns>Шаблоны аттрибутов</returns>
        private static CRFNgram[] ExtractAttributeTemplates( string text, Dictionary< char, int > columnIndexDictionary )
        {
            var attributeTemplateStrings = ExtractAttributeTemplateStrings( text );
            var split_chars              = new[] { ',' };
            var attributeTemplates       = new List< CRFNgram >( attributeTemplateStrings.Length );
            foreach ( var str in attributeTemplateStrings )
            {
                MatchCollection matchCollection = _TemplateRegex.Matches( str );
                if ( matchCollection.Count == 0 )
                    continue;

                var attributeTemplate = new List< CRFAttribute >( matchCollection.Count );
                foreach ( Match currentMatch in matchCollection )
                {
                    var oneTemplate = currentMatch.Groups[ Template ].Value;
                    var pair        = oneTemplate.Split( split_chars );

                    var attributeName = ParseAttributeName( pair[ 0 ] );
                    if ( attributeName.Length != 1 )
                    {
                        throw (new InvalidDataException( $"Attribute-name is not valid, must be one-char: '{attributeName}'" ));
                    }
                    var attributeNameChar = attributeName[ 0 ]; //char.ToUpperInvariant( attributeName[ 0 ] );
                    var position          = int.Parse( pair[ 1 ] );
                    var columnIndex       = columnIndexDictionary[ attributeNameChar ];                    

                    attributeTemplate.Add( new CRFAttribute( attributeNameChar, position, columnIndex ) );
                }
                attributeTemplates.Add( new CRFNgram( attributeTemplate.ToArray() ) );
            }
            return (attributeTemplates.ToArray());
        }

        /// <summary>
        /// Извлечь название аттрибута
        /// </summary>
        /// <param name="attr">Строка, содержащая название аттрибута</param>
        /// <returns>Название аттрибута</returns>
        private static string ParseAttributeName( string attr )
        {
            var startIndex = attr.IndexOf( '\'' ) + 1;
            var endIndex   = attr.IndexOf( '\'', startIndex);
            return (attr.Substring( startIndex, endIndex - startIndex ));
        }

        /// <summary>
        /// Извлечь названия столбцов
        /// </summary>
        /// <param name="text">Содержимое файла-шаблона</param>
        /// <returns>Названия столбцов</returns>
        private static char[] ExtractColumnNames( string text )
        {
            Match match = _FieldsRegex.Match( text );
            var columnNames     = match.Groups[ Fields ].Value.Split( ' ', '\t', '\n' );
            var columnNameChars = new char[ columnNames.Length ];
            for ( int i = 0; i < columnNames.Length; i++ )
            {
                var columnName = columnNames[ i ];
                if ( columnName.Length != 1 )
                {
                    throw (new InvalidDataException( $"Column-name is not valid, must be one-char: '{columnName}'" ));
                }
                columnNameChars[ i ] = columnName[ 0 ];
            }
            return (columnNameChars);
        }

        /// <summary>
        /// Извлечь строки, соответствующие шаблонам аттрибутов
        /// </summary>
        /// <param name="text">Содержимое файла-шаблона</param>
        /// <returns>Строки, соответствующие шаблонам аттрибутов</returns>
        private static string[] ExtractAttributeTemplateStrings( string text )
        {
            Match templatesMatch = _TemplatesRegex.Match( text );
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
            var dict = new Dictionary< char, int >( columnNames.Length );
            for ( var i = columnNames.Length - 1; 0 <= i; i--  )
			{
				dict[ columnNames[ i ] ] = i;
			}
            return (dict);
		}
    }
}
