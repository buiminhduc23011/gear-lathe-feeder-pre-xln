using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Desktop.App.Helpers;

internal static class NumericTextBoxInputHelper
{
    public static bool IsProposedTextValid(TextBox textBox, string input)
    {
        ArgumentNullException.ThrowIfNull(textBox);
        ArgumentNullException.ThrowIfNull(input);

        var proposed = textBox.Text
            .Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, input);

        return IsTextValid(proposed);
    }

    public static void HandlePaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        if (!e.DataObject.GetDataPresent(DataFormats.UnicodeText))
        {
            e.CancelCommand();
            return;
        }

        var pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
        if (!IsProposedTextValid(textBox, pastedText))
        {
            e.CancelCommand();
        }
    }

    private static bool IsTextValid(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        var decimalSeparators = GetAllowedDecimalSeparators();
        var hasDecimalSeparator = false;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (char.IsDigit(character))
            {
                continue;
            }

            if (character == '-')
            {
                if (index != 0)
                {
                    return false;
                }

                continue;
            }

            if (decimalSeparators.Contains(character))
            {
                if (hasDecimalSeparator)
                {
                    return false;
                }

                hasDecimalSeparator = true;
                continue;
            }

            return false;
        }

        return true;
    }

    private static HashSet<char> GetAllowedDecimalSeparators()
    {
        var separators = new HashSet<char> { '.' };
        var currentSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        if (currentSeparator.Length == 1)
        {
            separators.Add(currentSeparator[0]);
        }

        return separators;
    }
}
