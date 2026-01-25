using TL;

namespace Neurocommenting;

public class AccountConfig
{
    public string ApiId { get; init; }
    public string ApiHash { get; init; }
    public string Phone { get; init; }
    public string UserId { get; init; }
    public string SessionPath { get; init; }
    public string Proxy { get; init; }

    public string GetConfig(string key)
    {
        switch (key)
        {
            case "api_id": return ApiId;
            case "api_hash": return ApiHash;
            case "phone_number": return Phone;
            case "verification_code": 
                Console.Write($"Аккаунт с Id: {UserId} -> Введите код из телеграмм:"); 
                return Console.ReadLine();
            case "password": 
                Console.Write($"Аккаунт с Id: {UserId} -> Введите облачный пароль:");
                return Console.ReadLine();
            case "session_pathname": return SessionPath;
            case "user_id": return UserId;
            case "server_address": return "2>149.154.167.50:443";
            default: return null;
        }
    }
}