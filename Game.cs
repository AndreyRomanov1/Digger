using System;
using System.Text;
using Avalonia.Input;
using Digger.Architecture;

namespace Digger;

public static class Game
{
    private const string mapWithPlayerTerrain = @"
TTT T
TTP T
T T T
TT TT";

    private const string mapWithPlayerTerrainSackGold = @"
PTTGTT TS
TST  TSTT
TTTTTTSTT
T TSTS TT
T TTTG ST
TSTSTT TT";

    private const string mapWithPlayerTerrainSackGoldMonster = @"
PTTGTT TST
TST  TSTTM
TTT TTSTTT
T TSTS TTT
T TTTGMSTS
T TMT M TS
TSTSTTMTTT
S TTST  TG
 TGST MTTT
 T  TMTTTT";

    public static ICreature[,] Map;
    public static int Scores;
    public static bool IsOver;

    public static Key KeyPressed;
    public static int MapWidth => Map.GetLength(0);
    public static int MapHeight => Map.GetLength(1);

    public static void CreateMap()
    {
        Map = CreatureMapCreator.CreateMap(GenerateMap(20, 20, 20));
    }

    private static string GenerateMap(int width, int height, int maxMonsterCount)
    {
        var rand = new Random();
        var monsterCount = 0;
        var result = new StringBuilder();
        var playerX = rand.Next(0, width);
        var playerY = rand.Next(0, height);
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                if (i == playerX && j == playerY)
                    result.Append('P');
                else if (Math.Abs(i - playerX) < 2 && Math.Abs(j - playerY) < 2)
                    result.Append('T');
                else
                {
                    var r = rand.Next(0, 20);
                    switch (r)
                    {
                        case < 5:
                            result.Append(' ');
                            break;
                        case < 13:
                            result.Append('T');
                            break;
                        case < 14:
                            result.Append('S');
                            break;
                        case < 16:
                            result.Append('G');
                            break;
                        case < 17 when monsterCount < maxMonsterCount:
                            result.Append('M');
                            monsterCount++;
                            break;
                        case < 19 when monsterCount < maxMonsterCount:
                            result.Append('W');
                            monsterCount++;
                            break;
                        default:
                            result.Append(' ');
                            break;
                    }
                }
            }

            result.Append('\n');
        }

        return result.ToString();
    }
}