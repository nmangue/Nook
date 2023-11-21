﻿using System.Globalization;
using System.Text;

namespace Nook.CodeAnalysis.Language;

internal static class CSharpIdentifier
{
    // CSharp Spec §2.4.2
    private static bool IsIdentifierStart(char character)
    {
        return char.IsLetter(character) ||
            character == '_' ||
            CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.LetterNumber;
    }

    public static bool IsIdentifierPart(char character)
    {
        return char.IsDigit(character) ||
               IsIdentifierStart(character) ||
               IsIdentifierPartByUnicodeCategory(character);
    }

    private static bool IsIdentifierPartByUnicodeCategory(char character)
    {
        var category = CharUnicodeInfo.GetUnicodeCategory(character);

        return category == UnicodeCategory.NonSpacingMark || // Mn
            category == UnicodeCategory.SpacingCombiningMark || // Mc
            category == UnicodeCategory.ConnectorPunctuation || // Pc
            category == UnicodeCategory.Format; // Cf
    }

    public static string SanitizeIdentifier(string inputName)
    {
        if (string.IsNullOrEmpty(inputName))
        {
            return string.Empty;
        }

        var length = inputName.Length;
        var prependUnderscore = false;
        if (!IsIdentifierStart(inputName[0]) && IsIdentifierPart(inputName[0]))
        {
            length++;
            prependUnderscore = true;
        }

        var builder = new StringBuilder(length);
        if (prependUnderscore)
        {
            builder.Append('_');
        }

        for (var i = 0; i < inputName.Length; i++)
        {
            var ch = inputName[i];
            builder.Append(IsIdentifierPart(ch) ? ch : '_');
        }

        return builder.ToString();
    }
}
