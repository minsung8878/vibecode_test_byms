using System;

Random random = new Random();
int targetNumber = random.Next(1, 101); // 1 to 100 inclusive
int attempts = 0;
bool isCorrect = false;

Console.WriteLine("Welcome to the Number Guessing Game!");
Console.WriteLine("I have chosen a number between 1 and 100. Try to guess it!");

while (!isCorrect)
{
    Console.Write("Enter your guess: ");
    string? input = Console.ReadLine();

    if (input == null) break;

    if (!int.TryParse(input, out int userGuess))
    {
        Console.WriteLine("Invalid input. Please enter a numeric value.");
        continue;
    }

    attempts++;

    if (userGuess < targetNumber)
    {
        Console.WriteLine("Up!");
    }
    else if (userGuess > targetNumber)
    {
        Console.WriteLine("Down!");
    }
    else
    {
        isCorrect = true;
        Console.WriteLine($"Congratulations! You guessed it in {attempts} attempts.");
    }
}
