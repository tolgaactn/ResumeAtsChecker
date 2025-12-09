# 📄 Resume ATS Checker

> CV'nizin ATS (Başvuru Takip Sistemi) uyumluluğunu AI ile analiz edin

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=flat&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=flat&logo=postgresql&logoColor=white)
![OpenAI](https://img.shields.io/badge/OpenAI-412991?style=flat&logo=openai&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap&logoColor=white)

## 🎯 Proje Hakkında

Resume ATS Checker, iş arayanların CV'lerini ATS (Applicant Tracking System) sistemlerine göre analiz eden bir web uygulamasıdır. OpenAI'nin GPT-4o-mini modeli kullanılarak CV'ler analiz edilir ve kullanıcılara:

- **ATS Uyumluluk Skoru** (0-100)
- **Detaylı Özet** (Türkçe)
- **Eksik Anahtar Kelimeler**
- **İyileştirme Önerileri** (Türkçe)

sağlanır.

## ✨ Özellikler

- ✅ PDF formatında CV yükleme
- ✅ İş ilanı ile karşılaştırma
- ✅ AI destekli analiz (GPT-4o-mini)
- ✅ Gerçek zamanlı sonuç gösterimi
- ✅ Türkçe arayüz ve sonuçlar
- ✅ Responsive tasarım
- ✅ Database kayıt sistemi

## 🛠️ Teknolojiler

### Backend
- **Framework:** ASP.NET Core 8.0 Web API
- **Database:** PostgreSQL (Railway)
- **ORM:** Entity Framework Core
- **AI:** OpenAI API (gpt-4o-mini)
- **PDF İşleme:** PdfPig

### Frontend
- **HTML5, CSS3, JavaScript**
- **Bootstrap 5.3**
- **Font Awesome Icons**

### Architecture
- 3-Tier Architecture
- Dependency Injection
- Repository Pattern
- DTO Pattern

## 📋 Gereksinimler

- .NET 8.0 SDK
- PostgreSQL
- OpenAI API Key
- Git

## 🚀 Kurulum

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/tolgaactn/ResumeAtsChecker.git
cd ResumeAtsChecker
```

### 2. Yapılandırma Dosyası Oluşturun

`appsettings.Example.json` dosyasını `appsettings.json` olarak kopyalayın ve kendi bilgilerinizi girin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Port=YOUR_PORT;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR_OPENAI_API_KEY",
    "Model": "gpt-4o-mini",
    "MaxTokens": "2000"
  }
}
```

### 3. Database Migration

```bash
dotnet ef database update
```

### 4. Projeyi Çalıştırın

```bash
dotnet run
```

Uygulama `https://localhost:7231` adresinde çalışacaktır.

## 📸 Ekran Görüntüleri

### Ana Sayfa
![Ana Sayfa](screenshots/home.png)

### Analiz Sonuçları
![Sonuçlar](screenshots/results.png)

## 🔌 API Endpoints

### POST `/api/analysis/analyze`
CV analizi yapar.

**Request:**
- `resume` (IFormFile): PDF formatında CV
- `jobDescription` (string): İş ilanı açıklaması

**Response:**
```json
{
  "success": true,
  "analysisId": 5,
  "score": 85,
  "summary": "CV analizi...",
  "missingKeywords": ["keyword1", "keyword2"],
  "suggestions": ["öneri 1", "öneri 2"]
}
```

### GET `/api/analysis/{id}`
Analiz sonucunu getirir.

### POST `/api/analysis/extract-text`
PDF'den metin çıkarır (test amaçlı).

## 💾 Veritabanı Şeması

```sql
CREATE TABLE analyses (
    id SERIAL PRIMARY KEY,
    user_id VARCHAR(100),
    created_at TIMESTAMP,
    extracted_text TEXT,
    job_description TEXT,
    score INTEGER,
    summary TEXT,
    missing_keywords TEXT,
    suggestions TEXT,
    is_premium BOOLEAN
);
```

## 🔒 Güvenlik

- ⚠️ `appsettings.json` dosyası `.gitignore`'a eklenmiştir
- ⚠️ API anahtarlarınızı asla GitHub'a yüklemeyin
- ⚠️ Production ortamında environment variables kullanın

## 💰 Maliyet

- **OpenAI API:** ~$0.0002/analiz
- **Railway PostgreSQL:** Free tier (500MB)
- **Hosting:** Ücretsiz (Vercel/Railway)

## 📝 To-Do

- [ ] User authentication
- [ ] CV saklama özelliği
- [ ] Email ile rapor gönderme
- [ ] Word (.docx) dosya desteği
- [ ] LinkedIn entegrasyonu
- [ ] Premium özellikler
- [ ] Rate limiting

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add some amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👨‍💻 Geliştirici

**Tolga Açıtan**

- GitHub: [@tolgaactn](https://github.com/tolgaactn)
- LinkedIn: [Tolga Açıtan](https://www.linkedin.com/in/tolgaacitan/)
- Twitter: [@tacbuilds](https://twitter.com/tacbuilds)

## 🙏 Teşekkürler

- [OpenAI](https://openai.com/) - AI API
- [Railway](https://railway.app/) - PostgreSQL Hosting
- [PdfPig](https://github.com/UglyToad/PdfPig) - PDF İşleme

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
