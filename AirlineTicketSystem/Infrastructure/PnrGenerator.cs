namespace Airline_Ticket_System.Infrastructure;

/// <summary>Generates 6-character booking references (excludes ambiguous O/0, I/1).</summary>
public static class PnrGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string NewCode(Random? random = null)
    {
        random ??= Random.Shared;
        return string.Create(6, random, static (span, rnd) =>
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = Alphabet[rnd.Next(Alphabet.Length)];
        });
    }
}
