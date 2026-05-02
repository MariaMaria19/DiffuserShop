using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using DiffuserShop.Shared.Models;

namespace DiffuserShop.Client
{
    public partial class MainWindow : Window
    {
        private string _serverAddress = "127.0.0.1";
        private int _serverPort = 8888;
        private string _username;

        public MainWindow(string username)
        {
            InitializeComponent();
            _username = username;
            lblStatus.Text = $"Добро пожаловать, {username}!";
            LoadDiffusers();
        }

        private string SendCommand(string command, string data = "")
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(_serverAddress, _serverPort);
                using var stream = client.GetStream();

                string message = string.IsNullOrEmpty(data) ? command : command + "|" + data;
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                return $"ERROR|{ex.Message}";
            }
        }

        private void LoadDiffusers()
        {
            string response = SendCommand("GET_ALL");
            if (response.StartsWith("DATA|"))
            {
                string json = response.Substring(5);
                var diffusers = JsonSerializer.Deserialize<List<Diffuser>>(json);
                dgDiffusers.ItemsSource = diffusers;
                lblStatus.Text = $"Загружено товаров: {diffusers?.Count ?? 0}";
            }
            else
            {
                MessageBox.Show("Ошибка загрузки товаров", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDiffusers();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            var diffuser = new Diffuser
            {
                Name = txtName.Text,
                Scent = txtScent.Text,
                Price = decimal.Parse(txtPrice.Text),
                InStock = int.Parse(txtInStock.Text)
            };

            string jsonData = JsonSerializer.Serialize(diffuser);
            string response = SendCommand("ADD_ORM", jsonData);

            if (response.StartsWith("SUCCESS"))
            {
                MessageBox.Show("Товар добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDiffusers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Ошибка добавления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiffusers.SelectedItem is not Diffuser selected)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateForm()) return;

            selected.Name = txtName.Text;
            selected.Scent = txtScent.Text;
            selected.Price = decimal.Parse(txtPrice.Text);
            selected.InStock = int.Parse(txtInStock.Text);

            string jsonData = JsonSerializer.Serialize(selected);
            string response = SendCommand("UPDATE_ORM", jsonData);

            if (response.StartsWith("SUCCESS"))
            {
                MessageBox.Show("Товар обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDiffusers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Ошибка обновления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiffusers.SelectedItem is not Diffuser selected)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить \"{selected.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            string response = SendCommand("DELETE_ORM", selected.Id.ToString());

            if (response.StartsWith("SUCCESS"))
            {
                MessageBox.Show("Товар удалён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDiffusers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Ошибка удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadDiffusers();
                return;
            }

            string response = SendCommand("SEARCH_ORM", txtSearch.Text);
            if (response.StartsWith("DATA|"))
            {
                string json = response.Substring(5);
                var results = JsonSerializer.Deserialize<List<Diffuser>>(json);
                dgDiffusers.ItemsSource = results;
                lblStatus.Text = $"Найдено: {results?.Count ?? 0} товаров";
            }
            else
            {
                MessageBox.Show("Ошибка поиска", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgDiffusers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgDiffusers.SelectedItem is Diffuser selected)
            {
                txtName.Text = selected.Name;
                txtScent.Text = selected.Scent;
                txtPrice.Text = selected.Price.ToString();
                txtInStock.Text = selected.InStock.ToString();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!decimal.TryParse(txtPrice.Text, out _))
            {
                MessageBox.Show("Введите цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!int.TryParse(txtInStock.Text, out _))
            {
                MessageBox.Show("Введите количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtScent.Text = "";
            txtPrice.Text = "";
            txtInStock.Text = "";
        }
    }
}