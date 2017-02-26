using System;
using Windows.UI.Xaml.Controls;
using Todolist.DataModel;
using System.Diagnostics;

// Pour plus d'informations sur le modèle d'élément Boîte de dialogue de contenu, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace ToDoList
{
    public sealed partial class AddTodoDialog : ContentDialog
    {
        public ToDo NewTodo {get; set;}

        public AddTodoDialog()
        {
            this.InitializeComponent();
            EndDate.MinDate = DateTime.Now;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string subject = Subject.Text;
            string desc = Desc.Text;
            DateTimeOffset? endDate = EndDate.Date;
            bool state = (!State.IsChecked.HasValue || !State.IsChecked.Value) ? (false) : (true);
            this.NewTodo = new ToDo { Subject = subject, Description = desc, Due = endDate, Done = state };
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.NewTodo = null;
            this.Hide();
        }
    }
}
