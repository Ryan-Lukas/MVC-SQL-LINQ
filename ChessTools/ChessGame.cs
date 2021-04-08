using System;

namespace ChessTools
{
    public class ChessGame
    {
        private string eName;
        private string site;
        private string round;
        private string wPlayer;
        private string bPlayer;
        private int wElo;
        private int bElo;
        private string result;
        private string eventDate;
        private string moves;

        public string EName { get => eName; set => eName = value; }
        public string Site { get => site; set => site = value; }
        public string Round { get => round; set => round = value; }
        public string WPlayer { get => wPlayer; set => wPlayer = value; }
        public string BPlayer { get => bPlayer; set => bPlayer = value; }
        public int WElo { get => wElo; set => wElo = value; }
        public int BElo { get => bElo; set => bElo = value; }
        public string Result { get => result; set => result = value; }
        public string EventDate { get => eventDate; set => eventDate = value; }
        public string Moves { get => moves; set => moves = value; }
    }
}