

using System.Windows.Controls;
using System.Windows;
using System.Windows;

namespace aghmyeyes
{
    public partial class MainWindow : Window
    {
        private TaskManager taskManager;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize TaskManager with the CSV file path
            taskManager = new TaskManager("C:\\Users\\derrick\\Downloads\\FinalsCSV.csv");

            // Populate the ComboBox with actions
            cmbActions.ItemsSource = new string[] { "Create New Task", "Assign Task", "Add Comments to Task", "Complete Task", "Verify Task", "Delete Task" };

            // Display all tasks in the text block
            //ClearTextTasks();
            DisplayAllTasks();
        }

        private void ClearTextTasks()
        {
            txtTasks.Text = string.Empty;
        }

        private void DisplayAllTasks()
        {
            // Get all tasks from the TaskManager
            var tasks = taskManager.GetTasks();

            // Display tasks in the text block
            foreach (var task in tasks)
            {
                txtTasks.Text += $"Task Name: {task.TaskName}\n";
                txtTasks.Text += $"Description: {task.TaskDescription}\n";
                txtTasks.Text += $"Assigned To: {task.AssignedTo}\n";
                txtTasks.Text += $"Creation Time: {task.CreationTime}\n";
                txtTasks.Text += $"Time Assigned: {task.TimeAssigned}\n";
                txtTasks.Text += $"Time Completed: {task.TimeCompleted}\n";
                txtTasks.Text += $"Comments: {task.Comments}\n";
                txtTasks.Text += $"Verification Status: {task.VerificationStatus}\n";
                txtTasks.Text += $"Verification Time: {task.VerificationTime}\n";
                txtTasks.Text += $"Verifier Details: {task.VerifierDetails}\n";
                txtTasks.Text += $"Verification Comments: {task.VerificationComments}\n";
                txtTasks.Text += $"Task Status: {task.TaskStatus}\n\n";
            }
        }


        private void cmbActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Show or hide controls based on the selected action
            if (cmbActions.SelectedItem != null)
            {
                string selectedAction = cmbActions.SelectedItem.ToString();

                createTaskPanel.Visibility = selectedAction == "Create New Task" ? Visibility.Visible : Visibility.Collapsed;
                assignTaskPanel.Visibility = selectedAction == "Assign Task" ? Visibility.Visible : Visibility.Collapsed;
                addCommentsPanel.Visibility = selectedAction == "Add Comments to Task" ? Visibility.Visible : Visibility.Collapsed;
                completeTaskPanel.Visibility = selectedAction == "Complete Task" ? Visibility.Visible : Visibility.Collapsed;
                verifyTaskPanel.Visibility = selectedAction == "Verify Task" ? Visibility.Visible : Visibility.Collapsed;
                deleteTaskPanel.Visibility = selectedAction == "Delete Task" ? Visibility.Visible : Visibility.Collapsed;

                // Populate ComboBoxes based on the selected action
                switch (selectedAction)
                {
                    case "Delete Task":
                        PopulateComboBox(cmbTaskNameToDelete);
                        break;

                    case "Add Comments to Task":
                        PopulateComboBox(cmbTaskNameToAddComments);
                        break;

                    case "Complete Task":
                        PopulateComboBox(cmbTaskNameToComplete);
                        break;

                    case "Verify Task":
                        PopulateComboBox(cmbTaskNameToVerify);
                        break;

                    default:
                        break;
                }
            }

        }

        private void PopulateComboBox(ComboBox comboBox)
        {
            var taskNames = taskManager.GetTasks().Select(t => t.TaskName).ToList();
            comboBox.ItemsSource = taskNames;
        }

        private void PerformAction_Click(object sender, RoutedEventArgs e)
        {
            // Handle the selected action based on cmbActions.SelectedItem
            if (cmbActions.SelectedItem != null)
            {
                string selectedAction = cmbActions.SelectedItem.ToString();

                switch (selectedAction)
                {
                    case "Create New Task":
                        // Extract values from input controls
                        string newTaskName = txtNewTaskName.Text;
                        string newTaskDesc = txtNewTaskDescription.Text;
                        string comments = txtNewComments.Text;
                        string assignedTo = txtNewAssignedTo.Text;

                        // Check if the task name already exists
                        if (taskManager.GetTasks().Any(t => t.TaskName.Equals(newTaskName, StringComparison.OrdinalIgnoreCase)))
                        {
                            lblConcerns.Content = "Task with the same name already exists. Please choose a different name.";
                            return;
                        }

                        // Create a new task
                        taskManager.CreateNewTask(newTaskName, newTaskDesc, comments, assignedTo);
                        lblConcerns.Content = taskManager.GetConcerns();

                        // Display all tasks after creating a new one
                        ClearTextTasks();
                        DisplayAllTasks();
                        break;

                    case "Assign Task":
                        // Extract values from input controls
                        string taskNameToAssign = txtTaskNameToAssign.Text;
                        string assignedToUser = txtAssignedToUser.Text;
                        //int replaceAssignment = int.Parse(txtReplaceAssignment.Text);
                        int replaceAssignment = int.TryParse(txtReplaceAssignment.Text, out var replaceAssignmentValue) ? replaceAssignmentValue : 0;

                        // Check if the task name exists in the CSV file
                        if (!taskManager.TaskNameExists(taskNameToAssign))
                        {
                            lblConcerns.Content = "Task not found. Please enter a valid task name.";
                            return;
                        }

                        // Assign the task
                        taskManager.AssignTask(taskNameToAssign, assignedToUser, replaceAssignment);
                        lblConcerns.Content = taskManager.GetConcerns();

                        // Display all tasks after assigning
                        ClearTextTasks();
                        DisplayAllTasks();
                        break;

                    case "Add Comments to Task":
                        // Extract values from input controls
                        //string taskNameToAddComments = txtTaskNameToAddComments.Text;
                        string taskNameToAddComments = cmbTaskNameToAddComments.SelectedItem?.ToString();
                        string newComments = txtNewCommentshere.Text;
                        int replaceComments = int.TryParse(txtReplaceComments.Text, out var replaceCommentsValue) ? replaceCommentsValue : 0;

                        //<TextBox Name="txtTaskNameToAddComments" Width="200" Height="30"/>

                        // Check if the task name exists in the CSV file
                        if (!taskManager.TaskNameExists(taskNameToAddComments))
                        {
                            lblConcerns.Content = "Task not found. Please enter a valid task name.";
                            return;
                        }

                        // Add comments to the task
                        taskManager.AddComments(taskNameToAddComments, newComments, replaceComments);
                        lblConcerns.Content = taskManager.GetConcerns();

                        // Display all tasks after adding comments
                        ClearTextTasks();
                        DisplayAllTasks();
                        break;

                    // auh


                    case "Complete Task":
                        // Extract values from input controls
                        string taskNameToComplete = cmbTaskNameToComplete.SelectedItem?.ToString();

                        // Check if a task is selected
                        if (string.IsNullOrEmpty(taskNameToComplete))
                        {
                            lblConcerns.Content = "Please select a task to complete.";
                            return;
                        }

                        // Complete the task
                        taskManager.CompleteTask(taskNameToComplete);
                        lblConcerns.Content = taskManager.GetConcerns();

                        // Display all tasks after completing the task
                        ClearTextTasks();
                        DisplayAllTasks();
                        break;

                    case "Verify Task":
                        // Extract values from input controls
                        string taskNameToVerify = cmbTaskNameToVerify.SelectedItem?.ToString();
                        int verificationStatus = int.TryParse(txtVerificationStatus.Text, out var verificationStatusValue) ? verificationStatusValue : 1;
                        string verifierDetails = txtVerifierDetails.Text;
                        string verificationComments = txtVerificationComments.Text;

                        // Check if a task is selected
                        if (string.IsNullOrEmpty(taskNameToVerify))
                        {
                            lblConcerns.Content = "Please select a task to verify.";
                            return;
                        }

                        // Verify the task
                        taskManager.VerifyTask(verificationStatus, taskNameToVerify, verifierDetails, verificationComments);
                        lblConcerns.Content = taskManager.GetConcerns();

                        // Display all tasks after verifying the task
                        ClearTextTasks();
                        DisplayAllTasks();
                        break;

                    case "Delete Task":
                        // Extract values from input controls
                        string taskNameToDelete = cmbTaskNameToDelete.SelectedItem?.ToString();

                        // Check if the task name exists in the CSV file
                        if (!taskManager.TaskNameExists(taskNameToDelete))
                        {
                            lblConcerns.Content = "Task not found. Please enter a valid task name.";
                            return;
                        }

                        // Delete the task
                        taskManager.DeleteTask(taskNameToDelete);
                        lblConcerns.Content = taskManager.GetConcerns();

                        // Display all tasks after deleting the task
                        ClearTextTasks();
                        DisplayAllTasks();
                        break;

                    default:
                        break;
                }
            }
        }
    }
}




