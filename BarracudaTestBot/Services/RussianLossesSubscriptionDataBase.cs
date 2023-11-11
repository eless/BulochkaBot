using Microsoft.Data.SqlClient;

public class SubscriptionData
{
    public SubscriptionData(long chatId, bool isSubscribed, byte hour, byte minute)
    {
        ChatId = chatId;
        Subscribed = isSubscribed;
        Hour = hour;
        Minute = minute;
    }

    // Default constructor
    public SubscriptionData()
    {

    }

    public long ChatId { get; set; }
    public bool Subscribed { get; set; }
    public byte Hour { get; set; }
    public byte Minute { get; set; }
}

public class RussianLossesSubscriptionDataBase
{
    private readonly string _connectionString;
    private readonly string _lossesSubscriptionTable = "Subscriptions";

    public RussianLossesSubscriptionDataBase(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Can be used as a template for the new table
    private void CreateLossesSubscriptionIfNotExists(string tableName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Create a table if it doesn't exist
            string createTableQuery = $@"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}')
            CREATE TABLE {tableName} (
            ChatId INT,
            Subscribed BIT,
            Hour INT,
            Minute INT)";

            using (SqlCommand cmd = new SqlCommand(createTableQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void Subscribe(long chatId, bool enable, byte hour, byte minute)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string mergeQuery = $@"
            MERGE dbo.{_lossesSubscriptionTable} AS target
            USING (VALUES (@ChatId)) AS source (ChatId)
            ON target.ChatId = source.ChatId
            WHEN MATCHED THEN
                UPDATE SET Subscribed = @Subscribed, Hour = @Hour, Minute = @Minute
            WHEN NOT MATCHED THEN
                INSERT (ChatId, Subscribed, Hour, Minute)
                VALUES (@ChatId, @Subscribed, @Hour, @Minute);";

            using (SqlCommand cmd = new SqlCommand(mergeQuery, connection))
            {
                cmd.Parameters.AddWithValue("@ChatId", chatId);
                cmd.Parameters.AddWithValue("@Subscribed", enable);
                cmd.Parameters.AddWithValue("@Hour", hour);
                cmd.Parameters.AddWithValue("@Minute", minute);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public void Unsubscribe(long chatId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string deleteQuery = $"DELETE FROM {_lossesSubscriptionTable} WHERE ChatId = @ChatId";

            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection))
            {
                deleteCmd.Parameters.AddWithValue("@ChatId", chatId);
                deleteCmd.ExecuteNonQuery();
            }
        }
    }

    public List<SubscriptionData> GetAllLossesSubscriptions()
    {
        var subscriptions = new List<SubscriptionData>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string selectQuery = $"SELECT ChatId, Subscribed, Hour, Minute FROM {_lossesSubscriptionTable}";
            using (SqlCommand cmd = new SqlCommand(selectQuery, connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SubscriptionData data = new SubscriptionData();
                        data.ChatId = reader.GetInt64(reader.GetOrdinal("ChatId"));
                        data.Subscribed = reader.GetBoolean(reader.GetOrdinal("Subscribed"));
                        data.Hour = reader.GetByte(reader.GetOrdinal("Hour"));
                        data.Minute = reader.GetByte(reader.GetOrdinal("Minute"));
                        subscriptions.Add(data);
                    }
                }
            }
        }

        return subscriptions;
    }

    public SubscriptionData GetLossesSubscription(long chatId)
    {
        SubscriptionData data = new SubscriptionData(chatId, false, 0, 0);

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string selectQuery = $"SELECT Subscribed, Hour, Minute FROM {_lossesSubscriptionTable} WHERE ChatId = @ChatId";
            using (SqlCommand cmd = new SqlCommand(selectQuery, connection))
            {
                cmd.Parameters.AddWithValue("@ChatId", chatId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        data.Subscribed= reader.GetBoolean(reader.GetOrdinal("Subscribed"));
                        data.Hour = reader.GetByte(reader.GetOrdinal("Hour"));
                        data.Minute= reader.GetByte(reader.GetOrdinal("Minute"));
                    }
                }
            }
        }

        return data;
    }

    public void ClearLossesSubscriptionTable()
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string deleteQuery = $"DELETE FROM {_lossesSubscriptionTable}";

            using (SqlCommand cmd = new SqlCommand(deleteQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}