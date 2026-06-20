namespace InstructorPlus.Repositories;

public class DatabaseSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }

    public string ConnectionString => $"server={Host};port={Port};userid={Username};password={Password};database={Database}";
}