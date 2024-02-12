using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

public class TaskManager
{
    private string csvFilePath;

    private int concern;

    public TaskManager(string filePath)
    {
        csvFilePath = filePath;
        concern = 0;
    }

    public string GetConcerns()
    {
        if (concern == 0)
        {
            return "";
        }

        if (concern == 1)
        {
            return "Update did not happen because a task with that name already exists";
        }

        if (concern == 2)
        {
            return "File not found";
        }

        if (concern == 3)
        {
            return "A task couldn't be read";
        }

        if (concern == 4)
        {
            return "A task couldn't be written";
        }

        if (concern == 5)
        {
            return "The task is not For Verification";
        }

        if (concern == 6)
        {
            return "The task has already been completed";
        }

        if (concern == 7)
        {
            return "This class has already been verified";
        }

        concern = 0;
        return "you...";
    }

    public void AddTask(TaskItem task)
    {
        List<TaskItem> tasks = ReadTasks();

        // Check if a task with the same TaskName already exists
        if (tasks.Any(t => t.TaskName.Equals(task.TaskName, StringComparison.OrdinalIgnoreCase)))
        {
            concern = 1;
            return; // Exit 
        }

        task.CreationTime = DateTime.Now;

        tasks.Add(task);
        WriteTasks(tasks);
    }

    public List<TaskItem> GetTasks()
    {
        return ReadTasks();
    }

    public void AlterTask(string taskName, Action<TaskItem> alterationAction)
    {
        List<TaskItem> tasks = ReadTasks();
        TaskItem taskToAlter = tasks.Find(t => t.TaskName == taskName);

        if (taskToAlter != null)
        {
            // Store the original task name before alteration
            string originalTaskName = taskToAlter.TaskName;

            alterationAction(taskToAlter); // Apply the alteration action

            // Check if the task name has been changed to an existing task name
            if (tasks.Any(t => t.TaskName.Equals(taskToAlter.TaskName, StringComparison.OrdinalIgnoreCase)
                              && !t.TaskName.Equals(originalTaskName, StringComparison.OrdinalIgnoreCase)))
            {
                concern = 1;
                taskToAlter.TaskName = originalTaskName; // Revert task
            }

            WriteTasks(tasks); // Save tasks
        }
        else
        {
            concern = 2;
        }
    }

    public void CompleteTask(string taskName)
    {
        List<TaskItem> tasks = ReadTasks();
        TaskItem taskToComplete = tasks.Find(t => t.TaskName == taskName && (t.TaskStatus == "Open" || t.TaskStatus == "Assigned"));

        if (taskToComplete != null)
        {
            AlterTask(taskName, task =>
            {
                task.TaskStatus = "For Verification";
                task.VerificationStatus = "For Verification";
                task.TimeCompleted = DateTime.Now;
            });
        }
        else
        {
            concern = 6;
        }
    }

    public void CreateNewTask(string newTaskName, string newTaskDesc, string comments, string assignedTo)
    {
        string assign = assignedTo;

        TaskItem newTask;

        if (string.IsNullOrEmpty(assign))
        {
            newTask = new TaskItem
            {
                TaskName = newTaskName,
                TaskDescription = newTaskDesc,
                Comments = comments,
                TaskStatus = "Open",
                CreationTime = DateTime.Now
            };
        }
        else
        {
            newTask = new TaskItem
            {
                TaskName = newTaskName,
                TaskDescription = newTaskDesc,
                AssignedTo = assignedTo,
                Comments = comments,
                TaskStatus = "Assigned",
                TimeAssigned = DateTime.Now,
                CreationTime = DateTime.Now
            };
        }

        AddTask(newTask);
    }

    public void VerifyTask(int verified, string taskName, string verifierDetails, string verificationComments)
    {
        List<string> allowedStatuses = new List<string> { "For Verification", "For Revision" };

        // Check if the task has an allowed status
        TaskItem taskToVerify = GetTasks().FirstOrDefault(t => t.TaskName == taskName && allowedStatuses.Contains(t.TaskStatus));

        if (taskToVerify != null)
        {
            if (verified == 1)
            {
                AlterTask(taskName, task =>
                {
                    task.VerificationStatus = "Verified";
                    task.TaskStatus = "Closed";
                    task.VerificationTime = DateTime.Now;
                    task.VerifierDetails = verifierDetails;
                    task.VerificationComments = verificationComments;
                });
            }
            else if (verified == 2)
            {
                AlterTask(taskName, task =>
                {
                    task.VerificationStatus = "For Revision";
                    task.VerifierDetails = verifierDetails;
                    task.VerificationComments = verificationComments;
                    task.TaskStatus = "For Revision";
                });
            }
            else
            {
                // Handle other cases if needed
            }
        }
        else
        {
            concern = 5;
            // Handle the case where the task is not found or has a different status
        }
    }

    public void AssignTask(string taskName, string toassign, int replace)
    {
        if (replace == 1)
        {
            AlterTask(taskName, task =>
            {
                task.AssignedTo = toassign;
            });
        }
        else
        {
            AlterTask(taskName, task =>
            {
                task.AssignedTo += " and ";
                task.AssignedTo += $"{toassign}";
            });
        }
    }

    public void AddComments(string taskName, string nucomments, int replace)
    {
        if (replace == 1)
        {
            AlterTask(taskName, task =>
            {
                task.Comments = nucomments;
            });
        }
        else
        {
            AlterTask(taskName, task =>
            {
                task.Comments += " and ";
                task.Comments += $"{nucomments}";
            });
        }
    }

    private List<TaskItem> ReadTasks()
    {
        List<TaskItem> tasks = new List<TaskItem>();

        try
        {
            if (File.Exists(csvFilePath))
            {
                string[] lines = File.ReadAllLines(csvFilePath);

                foreach (string line in lines.Skip(1)) // Skip header line
                {
                    string[] values = line.Split(',');
                    if (values.Length == 12)
                    {
                        TaskItem task = new TaskItem
                        {
                            TaskName = values[0],
                            TaskDescription = values[1],
                            AssignedTo = values[2],
                            //TimeAssigned = DateTime.ParseExact(values[3], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            TimeAssigned = string.IsNullOrEmpty(values[3]) ? (DateTime?)null : DateTime.ParseExact(values[3], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            TimeCompleted = string.IsNullOrEmpty(values[4]) ? (DateTime?)null : DateTime.ParseExact(values[4], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            Comments = values[5],
                            VerificationStatus = values[6],
                            VerificationTime = string.IsNullOrEmpty(values[7]) ? (DateTime?)null : DateTime.ParseExact(values[7], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            VerifierDetails = values[8],
                            VerificationComments = values[9],
                            CreationTime = DateTime.ParseExact(values[10], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            TaskStatus = values[11]
                        };
                        tasks.Add(task);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            concern = 3;
            Console.WriteLine($"Error reading tasks: {ex.Message}");
        }

        return tasks;
    }

    private void WriteTasks(List<TaskItem> tasks)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(csvFilePath))
            {
                sw.WriteLine("taskname,taskdescription,assignedto,timeassigned,timecompleted,comments,verificationstatus,verificationtime,verifierdetails,verificationcomments,creationtime,taskstatus");

                foreach (TaskItem task in tasks)
                {
                    sw.WriteLine($"{task.TaskName},{task.TaskDescription},{task.AssignedTo}," +
                                 $"{task.TimeAssigned:yyyy-MM-dd HH:mm:ss},{task.TimeCompleted?.ToString("yyyy-MM-dd HH:mm:ss")}," +
                                 $"{task.Comments},{task.VerificationStatus},{task.VerificationTime?.ToString("yyyy-MM-dd HH:mm:ss")}," +
                                 $"{task.VerifierDetails},{task.VerificationComments},{task.CreationTime:yyyy-MM-dd HH:mm:ss},{task.TaskStatus}");
                }
            }
        }
        catch (Exception ex)
        {
            concern = 4;
            Console.WriteLine($"Error writing tasks: {ex.Message}");
        }
    }

    public bool TaskNameExists(string taskName)
    {
        return GetTasks().Any(t => t.TaskName.Equals(taskName, StringComparison.OrdinalIgnoreCase));
    }

    public void DeleteTask(string taskName)
    {
        List<TaskItem> tasks = ReadTasks();
        TaskItem taskToDelete = tasks.Find(t => t.TaskName == taskName);

        if (taskToDelete != null)
        {
            tasks.Remove(taskToDelete);
            WriteTasks(tasks);
        }
        else
        {
            concern = 2; // Task not found
        }
    }

    public void PerformUserTasks()
    {
        Console.WriteLine("Select an action:");
        Console.WriteLine("1. Create New Task");
        Console.WriteLine("2. Assign Task");
        Console.WriteLine("3. Add Comments to Task");
        Console.WriteLine("4. Complete Task");
        Console.WriteLine("5. Verify Task");

        int choice;
        if (int.TryParse(Console.ReadLine(), out choice))
        {
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Enter Task Name:");
                    string newTaskName = Console.ReadLine();

                    // Check if the task name already exists
                    if (GetTasks().Any(t => t.TaskName.Equals(newTaskName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine("Task with the same name already exists. Please choose a different name.");
                        return;
                    }

                    Console.WriteLine("Enter Task Description:");
                    string newTaskDesc = Console.ReadLine();

                    Console.WriteLine("Enter Comments:");
                    string comments = Console.ReadLine();

                    Console.WriteLine("Do you want to assign this task? (y/n)");
                    string assignChoice = Console.ReadLine().ToLower();

                    string assignedTo = string.Empty;

                    if (assignChoice == "y")
                    {
                        Console.WriteLine("Enter Assigned To:");
                        assignedTo = Console.ReadLine();
                    }

                    CreateNewTask(newTaskName, newTaskDesc, comments, assignedTo);
                    break;


                case 2:
                    Console.WriteLine("Enter Task Name:");
                    string taskNameToAssign = Console.ReadLine();

                    // Check if the task name exists in the CSV file
                    if (!TaskNameExists(taskNameToAssign))
                    {
                        Console.WriteLine("Task not found. Please enter a valid task name.");
                        return;
                    }

                    Console.WriteLine("Enter Assigned To:");
                    string assignedToUser = Console.ReadLine();

                    Console.WriteLine("Do you want to replace existing assignment? (1 for yes, 0 for no)");
                    int replaceAssignment = int.Parse(Console.ReadLine());

                    AssignTask(taskNameToAssign, assignedToUser, replaceAssignment);
                    break;

                case 3:
                    Console.WriteLine("Enter Task Name:");
                    string taskNameToAddComments = Console.ReadLine();


                    // Check if the task name exists in the CSV file
                    if (!TaskNameExists(taskNameToAddComments))
                    {
                        Console.WriteLine("Task not found. Please enter a valid task name.");
                        return;
                    }

                    Console.WriteLine("Enter Comments:");
                    string newComments = Console.ReadLine();

                    Console.WriteLine("Do you want to replace existing comments? (1 for yes, 0 for no)");
                    int replaceComments = int.Parse(Console.ReadLine());

                    AddComments(taskNameToAddComments, newComments, replaceComments);
                    break;

                case 4:
                    Console.WriteLine("Enter Task Name to Complete:");
                    string taskNameToComplete = Console.ReadLine();

                    // Check if the task name exists in the CSV file
                    if (!TaskNameExists(taskNameToComplete))
                    {
                        Console.WriteLine("Task not found. Please enter a valid task name.");
                        return;
                    }

                    CompleteTask(taskNameToComplete);
                    break;

                case 5:
                    Console.WriteLine("Enter Task Name to Verify:");
                    string taskNameToVerify = Console.ReadLine();

                    // Check if the task name exists in the CSV file
                    if (!TaskNameExists(taskNameToVerify))
                    {
                        Console.WriteLine("Task not found. Please enter a valid task name.");
                        return;
                    }

                    Console.WriteLine("Enter Verification Status (1 for Verified, 2 for For Revision):");
                    int verificationStatus = int.Parse(Console.ReadLine());

                    Console.WriteLine("Enter Verifier Details:");
                    string verifierDetails = Console.ReadLine();

                    Console.WriteLine("Enter Verification Comments:");
                    string verificationComments = Console.ReadLine();

                    VerifyTask(verificationStatus, taskNameToVerify, verifierDetails, verificationComments);
                    break;

                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter a number.");
        }

    }

}

public class TaskItem
{
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public string AssignedTo { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? TimeAssigned { get; set; }
    public DateTime? TimeCompleted { get; set; }
    public string Comments { get; set; }
    public string VerificationStatus { get; set; }
    public DateTime? VerificationTime { get; set; }
    public string VerifierDetails { get; set; }
    public string VerificationComments { get; set; }
    public string TaskStatus { get; set; }
}

