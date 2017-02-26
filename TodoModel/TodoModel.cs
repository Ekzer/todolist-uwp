using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Todolist.DataModel
{

    /// <summary>
    /// Todo's object.
    /// </summary>
    public class ToDo
    {
        private readonly string HTMLTemplate = "<html><head><title data-tag=\"to-do{0}\">{1}</title><meta name=\"created\" content=\"" + Utils.GetDate() + "\"/></head><body><p>{2}</p><p>{3}</p></body></html>";
        public string           Url { get; set; }
        public string           Subject { get; set; }
        public string           Description { get; set; }
        public DateTimeOffset?  Due { get; set; }
        public bool             Done { get; set; }
        public ToDo()
        {
            Done = false;
        }
        public String          Serialize()
        {
            return String.Format(this.HTMLTemplate, (this.Done == true) ? ":completed" : "", this.Subject, this.Description, this.Due.Value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Tasklist : ObservableCollection<ToDo>
    {
        public IEnumerable<ToDo> MatchTodos(String UserInput)
        {
            return this.Where(t =>
            t.Subject?.IndexOf(UserInput) > -1 ||
            t.Description?.IndexOf(UserInput) > -1 ||
            t.Url?.IndexOf(UserInput) > -1
            );
        }
    }
}
