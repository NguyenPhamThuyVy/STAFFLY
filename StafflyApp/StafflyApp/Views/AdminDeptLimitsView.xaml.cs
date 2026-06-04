using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StafflyApp.Models;
using StafflyApp.ViewModels;

namespace StafflyApp.Views
{
    public partial class AdminDeptLimitsView : UserControl
    {
        public AdminDeptLimitsView() => InitializeComponent();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AdminDeptLimitsViewModel vm) vm.LoadDeptLimitsData();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Cell, true);
                grid.CancelEdit(); 
                e.Handled = true;
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item is Department editedDept && this.DataContext is AdminDeptLimitsViewModel vm)
                {
                    var textBox = e.EditingElement as TextBox;
                    if (textBox != null && int.TryParse(textBox.Text, out int newValue))
                    {
                        editedDept.HeadcountLimit = newValue;
                        vm.SaveDeptLimitDataCommand.Execute(editedDept);
                    }
                }
            }
        }
    }
}