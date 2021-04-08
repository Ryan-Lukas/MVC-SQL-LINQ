using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ChessTools
{
    public static class PGNReader
    {
        
        
        public static List<ChessGame> Reader(string path)
        {
            List<ChessGame> data = new List<ChessGame>();

            try
            {
                var lines = File.ReadAllLines(path);
                
                // must have a count of 9 to have all fields required
                int count = 0;
                
                //space check for moves
                int space = 0;
                ChessGame currentGame = new ChessGame();
                
                foreach (var line in lines)
                {
                    if(space == 2 && count == 9)
                    {
                        space = 0;
                        count = 0;
                        data.Add(currentGame);
                        currentGame = new ChessGame();
                    }
                   
                    if (line.StartsWith("[EventDate"))
                    {
                        //check date to replace to 0000-00-00 else regular date
                        if(line.Substring(12, line.Length - 14).Contains("?"))
                        {
                            currentGame.EventDate = "0000-00-00";
                        }
                        else
                        {
                            currentGame.EventDate = line.Substring(12, line.Length - 14);
                        }
                        count += 1;
                    }
                    else if (line.StartsWith("[Event"))
                    {
                        currentGame.EName = line.Substring(8, line.Length - 10);
                        count += 1;
                    }

                    else if (line.StartsWith("[Site"))
                    {
                        currentGame.Site = line.Substring(7, line.Length - 9);
                        count += 1;
                    }

                    else if (line.StartsWith("[Round"))
                    {
                        currentGame.Round = line.Substring(8, line.Length - 10);
                        count += 1;
                    }
                    
                    else if (line.StartsWith("[WhiteElo"))
                    {
                        string elo = line.Substring(11, line.Length - 13);
                        currentGame.WElo = int.Parse(elo);
                        count += 1;
                    }
                    
                    else if (line.StartsWith("[BlackElo"))
                    {
                        string elo = line.Substring(11, line.Length - 13);
                        currentGame.BElo = int.Parse(elo);
                        count += 1;
                    }
                   
                    else if (line.StartsWith("[White"))
                    {
                        currentGame.WPlayer = line.Substring(8, line.Length - 10);
                        count += 1;
                    }
                   
                    else if (line.StartsWith("[Black"))
                    {
                        currentGame.BPlayer = line.Substring(8, line.Length - 10);
                        count += 1;
                    }
                  
                    else if (line.StartsWith("[Result"))
                    {
                        string result = line.Substring(9, line.Length - 11);

                        if (String.Equals(result, "1-0"))
                        {
                            currentGame.Result = "W";
                        }
                        else if (String.Equals(result, "0-1"))
                        {
                            currentGame.Result = "B";
                        }
                        else
                        {
                            currentGame.Result = "D";
                        }
                        count += 1;
                    }

                    if (!String.Equals(line,"") &&  space == 1 )
                    {
                        //needed space sometimes after line of moves
                        //after '.' no space, no space if space at end of line
                        if(line.Substring(line.Length-1).Contains(".") || line.Substring(line.Length - 1).Contains(" "))
                        {
                            currentGame.Moves = currentGame.Moves + line;
                        }
                        else
                        {
                            currentGame.Moves = currentGame.Moves + line + " ";
                        }
                        
                    }

                    if (String.Equals(line, "")) { space += 1; }

                }
                data.Add(currentGame);
            }
            catch(Exception e ) { }


            return data;
        }
    }
}
