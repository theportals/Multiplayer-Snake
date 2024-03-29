using Microsoft.Xna.Framework.Input;

namespace Client.Util;

public class TextInputUtil
{
    public static string getCharacter(Keys key)
    {
        var state = Keyboard.GetState();
        var shift = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift) || state.CapsLock;
        return key switch
        {
            Keys.A => shift ? "A" : "a",
            Keys.B => shift ? "B" : "b",
            Keys.C => shift ? "C" : "c",
            Keys.D => shift ? "D" : "d",
            Keys.E => shift ? "E" : "e",
            Keys.F => shift ? "F" : "f",
            Keys.G => shift ? "G" : "g",
            Keys.H => shift ? "H" : "h",
            Keys.I => shift ? "I" : "i",
            Keys.J => shift ? "J" : "j",
            Keys.K => shift ? "K" : "k",
            Keys.L => shift ? "L" : "l",
            Keys.M => shift ? "M" : "m",
            Keys.N => shift ? "N" : "n",
            Keys.O => shift ? "O" : "o",
            Keys.P => shift ? "P" : "p",
            Keys.Q => shift ? "Q" : "q",
            Keys.R => shift ? "R" : "r",
            Keys.S => shift ? "S" : "s",
            Keys.T => shift ? "T" : "t",
            Keys.U => shift ? "U" : "u",
            Keys.V => shift ? "V" : "v",
            Keys.W => shift ? "W" : "w",
            Keys.X => shift ? "X" : "x",
            Keys.Y => shift ? "Y" : "y",
            Keys.Z => shift ? "Z" : "z",
            
            Keys.D1 => shift ? "!" : "1",
            Keys.D2 => shift ? "@" : "2",
            Keys.D3 => shift ? "#" : "3",
            Keys.D4 => shift ? "$" : "4",
            Keys.D5 => shift ? "%" : "5",
            Keys.D6 => shift ? "^" : "6",
            Keys.D7 => shift ? "&" : "7",
            Keys.D8 => shift ? "*" : "8",
            Keys.D9 => shift ? "(" : "9",
            Keys.D0 => shift ? ")" : "0",
            
            Keys.OemMinus => shift ? "_" : "-",
            Keys.OemPlus => shift ? "+" : "=",
            Keys.OemTilde => shift ? "~" : "`",

            _ => ""
        };
    }
}