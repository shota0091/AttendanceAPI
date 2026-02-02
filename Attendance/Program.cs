using Attendance.Data;
using Attendance.Repositories;// ※フォルダ名が Repositories なら s をつける
using Attendance.Services;
using Microsoft.EntityFrameworkCore;
// using MyAttendanceApi.Repositories; // ←これは不要なら消す

var builder = WebApplication.CreateBuilder(args);

// ■ サービス登録エリア
builder.Services.AddControllers();

// ★ここを変更！ (OpenApi → SwaggerGen)
// これが「画面」を作るための必須セットです
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB設定
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI登録
// ※注意: IAttendanceRepository のフォルダ名や名前空間が合ってるか要確認
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

var app = builder.Build();

// ■ パイプライン設定エリア
if (app.Environment.IsDevelopment())
{
    // ★ここを変更！ (MapOpenApi → UseSwagger & UseSwaggerUI)
    // これで /swagger/index.html が表示されるようになります
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();