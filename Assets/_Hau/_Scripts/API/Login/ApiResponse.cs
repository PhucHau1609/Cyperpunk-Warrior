using Newtonsoft.Json;
using System;

[Serializable]
public class ApiResponse<T>
{
    [JsonProperty("isSuccess")] public bool isSuccess;
    [JsonProperty("IsSuccess")] public bool IsSuccessPascal;

    [JsonProperty("notification")] public string notification;
    [JsonProperty("Notification")] public string NotificationPascal;

    [JsonProperty("data")] public T data;

    // helper để dùng gọn
    public bool Success => isSuccess || IsSuccessPascal;
    public string Message => !string.IsNullOrEmpty(notification) ? notification : NotificationPascal;
}
