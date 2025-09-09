using System;
using System.ComponentModel;

namespace NV.CT.JobViewer.Models;

public class ComboxItem : INotifyPropertyChanged, IEquatable<ComboxItem>
{
    private string? _text;
    private string? _value;
    public string Text
    {
        get
        {
            return string.IsNullOrEmpty(_text) ? string.Empty : _text;
        }
        set
        {
            _text = value;
            OnChangedProperty("Text");
        }
    }
    public string Value
    {
        get
        {
            return string.IsNullOrEmpty(_value) ? string.Empty : _value;
        }
        set
        {
            this._value = value;
            OnChangedProperty("Value");
        }
    }

    public ComboxItem()
    { 
    
    }
    public ComboxItem(string text, string value)
    {
        this._text = text;
        this._value = value;
    }

    //public override bool Equals(object? obj)
    //{
    //    return base.Equals(obj);
    //}
    //public override bool Equals(object other)
    //{
    //    //if(obj is ComboxItem other)
    //    //{
    //    //    return Equals(other);
    //    //}
    //    //return false;
    //    if (this.Text == ((ComboxItem)other).Text && this.Value == ((ComboxItem)other).Value)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnChangedProperty(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    bool IEquatable<ComboxItem>.Equals(ComboxItem? other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
