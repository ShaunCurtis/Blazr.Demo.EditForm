namespace Blazr.UI;

public class ModalOptions : IEnumerable<KeyValuePair<string, object>>
{
    /// <summary>
    /// List of options
    /// </summary>
    public static readonly string __Width = "Width";
    public static readonly string __ID = "ID";
    public static readonly string __ExitOnBackGroundClick = "ExitOnBackGroundClick";

    private Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (var item in Parameters)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();

    public T? Get<T>(string key)
    {
        if (this.Parameters.ContainsKey(key))
        {
            if (this.Parameters[key] is T t) return t;
        }
        return default;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        value = default;
        if (this.Parameters.ContainsKey(key))
        {
            if (this.Parameters[key] is T t)
            {
                value = t;
                return true;
            }
        }
        return false;
    }

    public bool Set(string key, object value)
    {
        if (this.Parameters.ContainsKey(key))
        {
            this.Parameters[key] = value;
            return false;
        }
        this.Parameters.Add(key, value);
        return true;
    }
}

