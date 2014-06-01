using System;

namespace Shared.GameMechanics
{
    public class Card
    {
        public readonly CardColor Color;
        public readonly string OracleText;
        public readonly CardType Type;
        public Card(CardColor color,CardType type,string oracleText)
        {
            if (oracleText == null) throw new ArgumentNullException("oracleText");
            Color = color;
            OracleText = oracleText;
            Type = type;
        }
    }

    public enum CardType
    {
        Nonland,
        Land
    }
    
    public enum CardColor
    {
        Colorless,
        White,
        Blue,
        Black,
        Red,
        Green
    }
}