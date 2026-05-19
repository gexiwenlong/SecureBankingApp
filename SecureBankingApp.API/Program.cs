using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SecureBankingApp.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 从环境变量读取 JWT Secret（安全方式）
// =============================================
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

// 如果环境变量没有设置，检查是否是开发环境
if (string.IsNullOrEmpty(jwtSecret))
{
    if (builder.Environment.IsDevelopment())
    {
        // 仅用于本地开发测试，永不用于生产环境
        jwtSecret = "DevOnlySecretKey_NotForProduction_32Chars!!";
        Console.WriteLine("警告：使用开发环境 JWT 密钥，请勿用于生产环境！");
    }
    else
    {
        // 生产环境必须设置 JWT_SECRET
        throw new InvalidOperationException("JWT_SECRET 环境变量未设置！");
    }
}

// 将字符串密钥转换为字节数组
var key = Encoding.UTF8.GetBytes(jwtSecret);

// 注册自定义服务
builder.Services.AddScoped<IPasswordService, PasswordService>();

// 添加控制器和 API 文档
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "安全银行 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "请输入您的 JWT 令牌"
    });
});

// 配置 JWT 认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
