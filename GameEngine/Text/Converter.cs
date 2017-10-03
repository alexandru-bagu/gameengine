using OpenTK.Input;

namespace GameEngine.Text
{
    public class Converter
    {
        public static char Convert(Key key, Input input)
        {
            char c = '\0';
            bool shift = input.KeyDown(Key.ShiftLeft) || input.KeyDown(Key.ShiftRight);
            bool upperCase = shift ^ input.CapsLock;

            if ((key >= Key.A) && (key <= Key.Z))
            {
                if (upperCase) c = (char)((int)'A' + (int)(key - Key.A));
                else c = (char)((int)'a' + (int)(key - Key.A));
            }
            else if ((key >= Key.Number0) && (key <= Key.Number9))
            {
                if (shift)
                {
                    if (key == Key.Number0) c = ')';
                    else if (key == Key.Number1) c = '!';
                    else if (key == Key.Number2) c = '@';
                    else if (key == Key.Number3) c = '#';
                    else if (key == Key.Number4) c = '$';
                    else if (key == Key.Number5) c = '%';
                    else if (key == Key.Number6) c = '^';
                    else if (key == Key.Number7) c = '&';
                    else if (key == Key.Number8) c = '*';
                    else if (key == Key.Number9) c = '(';
                }
                else
                    c = (char)((int)'0' + (int)(key - Key.Number0));
            }
            else if ((key >= Key.Keypad0) && (key <= Key.Keypad9))
            {
                c = (char)((int)'0' + (int)(key - Key.Keypad0));
            }
            else if (key == Key.Space) c = ' ';
            else if (key == Key.Tab) c = '\t';
            else if (key == Key.Enter || key == Key.KeypadEnter) c = '\n';
            else if (key == Key.KeypadPlus) c = '+';
            else if (key == Key.KeypadMinus) c = '-';
            else if (key == Key.KeypadPeriod) c = '.';
            else if (key == Key.KeypadDivide) c = '/';
            else if (key == Key.KeypadMultiply) c = '*';
            if (!shift)
            {
                if (key == Key.BracketLeft) c = '[';
                else if (key == Key.BracketRight) c = ']';
                else if (key == Key.Comma) c = ',';
                else if (key == Key.Period) c = '.';
                else if (key == Key.Quote) c = '\'';
                else if (key == Key.Tilde || key == Key.Grave) c = '`';
                else if (key == Key.Slash) c = '\\';
                else if (key == Key.BackSlash) c = '/';
                else if (key == Key.Semicolon) c = ';';
                else if (key == Key.Plus) c = '=';
                else if (key == Key.Minus) c = '-';
            }
            else
            {
                if (key == Key.BracketLeft) c = '{';
                else if (key == Key.BracketRight) c = '}';
                else if (key == Key.Comma) c = '<';
                else if (key == Key.Period) c = '>';
                else if (key == Key.Quote) c = '"';
                else if (key == Key.Tilde || key == Key.Grave) c = '~';
                else if (key == Key.Slash) c = '?';
                else if (key == Key.BackSlash) c = '|';
                else if (key == Key.Semicolon) c = ':';
                else if (key == Key.Plus) c = '+';
                else if (key == Key.Minus) c = '_';
            }

            return c;
        }
    }
}
