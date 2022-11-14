namespace Blazr.App.UI;

public partial class WeatherForecastListForm : ComponentBase
{
    [Inject] private WeatherForecastsViewService? listViewService { get; set; }

    [Inject] private WeatherForecastViewService? recordViewService { get; set; }

    [Inject] private NavigationManager? navigationManager { get; set; }

    private bool isLoading => listViewService!.Records is null;

    public bool IsModal { get; set; }

    private IModalDialog? modalDialog { get; set; }

    private ComponentState loadState => isLoading ? ComponentState.Loading : ComponentState.Loaded;

    protected override void OnInitialized()
        => this.listViewService!.ListChanged += this.OnListChanged;

    private async Task DeleteRecord(Guid Id)
        => await this.recordViewService!.DeleteRecordAsync(Id);

    private async Task EditRecord(Guid Id)
    {
        if (this.IsModal)
        {
            var options = new ModalOptions();
            options.Set("Id",Id);
            await this.modalDialog!.ShowAsync<WeatherForecastEditForm>(options);
        }
        else
            this.navigationManager!.NavigateTo($"/WeatherForecast/Edit/{Id}");
    }

    private async Task AddRecordAsync()
        => await this.recordViewService!.AddRecordAsync(
            new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Id = Guid.NewGuid(),
                Summary = "Balmy",
                TemperatureC = 14
            });

    private void OnListChanged(object? sender, EventArgs e)
        => this.InvokeAsync(this.StateHasChanged);

    public void Dispose()
        => this.listViewService!.ListChanged -= this.OnListChanged;
}

