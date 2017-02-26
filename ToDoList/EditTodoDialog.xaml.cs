using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Todolist.DataModel;
using Todolist.API;
using System.Diagnostics;

// Pour plus d'informations sur le modèle d'élément Boîte de dialogue de contenu, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace ToDoList
{
    public sealed partial class EditTodoDialog : ContentDialog
    {
        public ToDo task;
        private OneNote OneNoteAPI;

        public EditTodoDialog(ToDo selected, OneNote OneNoteAPI)
        {
            this.InitializeComponent();
            this.task = selected;
            this.EndDate.MinDate = DateTime.Now;
            this.EndDate.Date = task.Due.Value;
            this.Subject.Text = task.Subject;
            this.Desc.Text = task.Description;
            this.State.IsChecked = task.Done;
            this.OneNoteAPI = OneNoteAPI;
        }

        public async void EditTodoDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            task.Subject = Subject.Text;
            task.Description = Desc.Text;
            task.Due = EndDate.Date;
            task.Done = (!State.IsChecked.HasValue || !State.IsChecked.Value) ? (false) : (true);
            task.Url = await this.OneNoteAPI.UpdateNote(task.Url, task.Serialize());
        }

        private void EditTodoDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
