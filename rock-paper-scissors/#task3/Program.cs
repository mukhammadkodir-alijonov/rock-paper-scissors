using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class Game
{
    private string[] moves;
    private Crypto crypto;
    private Table table;

    public Game(string[] args)
    {
        if (args.Length < 3 || args.Length % 2 == 0)
        {
            throw new ArgumentException("Invalid number of arguments. Please provide an odd number of arguments >= 3.");
        }

        this.moves = args;
        this.crypto = new Crypto();
        this.table = new Table(moves);
    }

    public void Play()
    {
        int computerMoveIndex = new Random().Next(moves.Length);
        string computerMove = moves[computerMoveIndex];
        string key = crypto.GenerateKey();
        string hmac = crypto.CalculateHMAC(key, computerMove);

        Console.WriteLine($"HMAC: {hmac}");
        Console.WriteLine("Please choose your move:");

        int userMoveIndex = GetUserMove();
        if (userMoveIndex == -1)
        {
            Console.WriteLine("Exiting the game.");
            return;
        }

        Console.WriteLine($"Your move: {moves[userMoveIndex]}");
        Console.WriteLine($"Computer move: {computerMove}");

        if (userMoveIndex == computerMoveIndex)
        {
            Console.WriteLine("Draw!");
        }
        else if ((userMoveIndex - computerMoveIndex + moves.Length) % moves.Length <= moves.Length / 2)
        {
            Console.WriteLine("You win!");
        }
        else
        {
            Console.WriteLine("Computer wins!");
        }

        Console.WriteLine($"HMAC key: {key}");
        Console.WriteLine("A new game: - - - - - - - - - - - - >");
        Play();
    }

    private int GetUserMove()
    {
        while (true)
        {
            Console.WriteLine("Available moves:");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {moves[i]}");
            }
            Console.WriteLine("0 - exit");
            Console.WriteLine("? - help");

            string input = Console.ReadLine();

            if (input == "0")
            {
                return -1;
            }

            if (input == "?")
            {
                table.Print();
                continue;
            }

            if (int.TryParse(input, out int move) && move >= 1 && move <= moves.Length)
            {
                return move - 1;
            }

            Console.WriteLine("Invalid input. Please enter a number between 1 and {0}, or '0' to exit, or '?' for help.", moves.Length);
        }
    }
}

public class Move
{
    public string Name { get; }
    public string[] Beats { get; }

    public Move(string name, string[] beats)
    {
        this.Name = name;
        this.Beats = beats;
    }
}

public class Crypto
{
    public string GenerateKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] key = new byte[32];
            rng.GetBytes(key);
            return BitConverter.ToString(key).Replace("-", "").ToLower();
        }
    }

    public string CalculateHMAC(string key, string message)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}

public class Table
{
    private Move[] moves;

    public Table(string[] moveNames)
    {
        this.moves = new Move[moveNames.Length];

        for (int i = 0; i < moveNames.Length; i++)
        {
            string[] beats = new string[moveNames.Length / 2];
            for (int j = 0; j < beats.Length; j++)
            {
                beats[j] = moveNames[(i + j + 1) % moveNames.Length];
            }

            moves[i] = new Move(moveNames[i], beats);
        }
    }

    public void Print()
    {
        Console.WriteLine("Move\t\t" + string.Join("\t", moves.Select(m => m.Name)));

        foreach (Move move in moves)
        {
            Console.Write(move.Name + "\t\t");
            foreach (Move other in moves)
            {
                if (other == move)
                {
                    Console.Write("Draw\t");
                }
                else if (move.Beats.Contains(other.Name))
                {
                    Console.Write("Win\t");
                }
                else
                {
                    Console.Write("Lose\t");
                }
            }
            Console.WriteLine();
        }
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            new Game(args).Play();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
