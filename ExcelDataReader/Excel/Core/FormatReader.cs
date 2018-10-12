﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Excel.Core
{
	public class FormatReader
	{
		private const char escapeChar = '\\';
		public string FormatString { get; set; }
		public FormatReader()
		{
			
		}

		public bool IsDateFormatString()
		{
			//it is a date if it contains y,m,d,s,h but only
			//if the term is not contained in [ ] (i.e. a color) e.g. [Red] 
			//or the term is not in quotes "d" (means display d)
			//or the term is not preceded by a backslash e.g. \d (means display d)

			var dateChars = new char[] { 'y', 'm', 'd', 's', 'h', 'Y', 'M', 'D', 'S', 'H' };
			if (FormatString.IndexOfAny(dateChars) >= 0)
			{
				var isDate = false;
				//it is a date candidate
				foreach (var dateChar in dateChars)
				{
					//perform our checks for each occurance of the character
					var pos = FormatString.IndexOf(dateChar);
					while (pos > -1)
					{

						//could probably do this with regex...
						if (!IsSurroundedByBracket(dateChar, pos) &&
								!IsPrecededByBackSlash(dateChar, pos) &&
								!IsSurroundedByQuotes(dateChar, pos))
							return true;

						//get next occurance
						pos = FormatString.IndexOf(dateChar, pos + 1);
					}

				}
			}

			return false;
		}

		private bool IsSurroundedByQuotes(char dateChar, int pos)
		{
			//char was at end then can't be surrounded
			if (pos == FormatString.Length - 1)
				return false;

			//is there an odd number of quotes after pos
			//is there an odd number of quotes before pos
			int numAfter = NumberOfUnescapedOccurances('"', FormatString.Substring(pos + 1));
			int numBefore = NumberOfUnescapedOccurances('"', FormatString.Substring(0, pos));

			return numAfter % 2 == 1 && numBefore % 2 == 1;

		}

		private bool IsPrecededByBackSlash(char dateChar, int pos)
		{
			if (pos == 0)
				return false;


			char lastChar = FormatString[pos - 1];
			if (lastChar.CompareTo(escapeChar) == 0)
				return true;

			return false;
		}

		private bool IsSurroundedByBracket(char dateChar, int pos)
		{
			//char was at end then can't be surrounded
			if (pos == FormatString.Length - 1)
				return false;

			//if number of [ before minus number of [ before is odd and 
			//if number of ] after minus number of ] after is odd then it is surrounded 
			int numOpenBefore = NumberOfUnescapedOccurances('[', FormatString.Substring(0, pos));
			int numClosedBefore = NumberOfUnescapedOccurances(']', FormatString.Substring(0, pos));
			numOpenBefore = numOpenBefore - numClosedBefore;

			int numOpenAfter = NumberOfUnescapedOccurances('[', FormatString.Substring(pos + 1));
			int numClosedAfter = NumberOfUnescapedOccurances(']', FormatString.Substring(pos + 1));
			numClosedAfter = numClosedAfter - numOpenAfter;

			return numOpenBefore % 2 == 1 && numClosedAfter % 2 == 1;
		}

		private int NumberOfUnescapedOccurances(char value, string src)
		{
			var numOccurances = 0;
			char lastChar = Char.MinValue;
			foreach (char c in src)
			{
				if (c != value)
					continue;

				if (lastChar != Char.MinValue && lastChar.CompareTo(escapeChar) == 0) //ignore if escaped
					continue;

				numOccurances++;
				lastChar = c;
			}

			return numOccurances;
		}


	}
}
