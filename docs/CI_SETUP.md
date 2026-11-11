# Rugtopia CI kurulumu (Mac yok)

Bu depo **herkese açık**. Ücretsiz macOS runner dakikası için böyle duruyor. Lisans dosyası, sertifika, `.p8`, provisioning profile ve şifre **repoya yazılmaz**. Hepsi GitHub → Settings → Secrets and variables → Actions altına secret olarak konur.

Unity derlemesi Ubuntu'da, imza ve TestFlight yüklemesi GitHub'ın macOS runner'ında yapılır. Kendi bilgisayarınızda Xcode veya Unity Editor açmanız gerekmez. Unity Personal lisansı için Unity Hub yeter (Windows veya Linux).

Belgeler 11 Kasım 2026'da şu sayfalarla karşılaştırıldı:

- https://game.ci/docs/github/activation
- https://game.ci/docs/github/builder
- https://game.ci/docs/github/test-runner
- https://docs.fastlane.tools/app-store-connect-api/
- https://docs.fastlane.tools/actions/match/
- https://developer.apple.com/documentation/appstoreconnectapi/creating-api-keys-for-app-store-connect-api

## Ne otomatik çalışır

| Workflow | Ne zaman | Secret yokken |
|---|---|---|
| `core-tests` | Her push ve pull request | `dotnet test` çalışır, yeşil olmalı |
| `unity-build` | `main`'e push, pull request, elle çalıştırma | Yeşil kalır, logda "Unity build skipped" yazar |
| `ios-testflight` | Elle çalıştırma ve `v*` etiketleri (ör. `v0.1.0`) | Yeşil kalır, logda "TestFlight skipped" yazar |

`unity-build`, GameCI `unity-test-runner@v4` ile EditMode testini, `unity-builder@v6` ile iOS Xcode projesini üretir. Editör sürümü `ProjectSettings/ProjectVersion.txt` içindeki `6000.3.0f1` değeridir. Bu sürümün `unityci/editor:ubuntu-6000.3.0f1-ios-3` imajı yayında. Sürümü değiştirirseniz Docker Hub'da `unityci/editor` altında `ubuntu-<sürüm>-ios-3` etiketinin olduğundan emin olun.

`ios-testflight` fastlane **match** kullanır. Sertifika bu public repoda durmaz. Ayrı, **private** bir git deposuna şifreli yazılır. İlk çalıştırmada macOS runner sertifikayı Apple'da oluşturur; sizin Mac'iniz gerekmez.

## Eklenecek secret'lar

İsimler birebir böyle olmalı. Başka isim workflow tarafından okunmaz.

### Unity Personal (üçü birden)

| Secret | İçinde ne var |
|---|---|
| `UNITY_LICENSE` | `Unity_lic.ulf` dosyasının **tam metni**. Base64 yapmayın. |
| `UNITY_EMAIL` | Unity hesabının e-postası |
| `UNITY_PASSWORD` | Unity hesabının şifresi |

### Unity Pro (Personal yerine; üçü birden)

Personal secret'larını kullanmayın.

| Secret | İçinde ne var |
|---|---|
| `UNITY_SERIAL` | `XX-XXXX-XXXX-XXXX-XXXX-XXXX` biçimindeki seri |
| `UNITY_EMAIL` | Unity hesabının e-postası |
| `UNITY_PASSWORD` | Unity hesabının şifresi |

### TestFlight (hepsi gerekli)

| Secret | İçinde ne var |
|---|---|
| `APPSTORE_KEY_ID` | API anahtarının Key ID'si (10 karakter) |
| `APPSTORE_ISSUER_ID` | Integrations sayfasının üstündeki Issuer ID (UUID) |
| `APPSTORE_P8` | `.p8` dosyasının **base64** hali, tek satır |
| `APPLE_TEAM_ID` | Apple Developer Team ID (10 karakter) |
| `MATCH_GIT_URL` | Private sertifika deposunun HTTPS adresi |
| `MATCH_PASSWORD` | Match arşivini şifreleyen parola |
| `MATCH_GIT_TOKEN` | Yalnızca o private depoya yazabilen GitHub token |

Workflow, `MATCH_GIT_TOKEN` değerinden match'in beklediği `MATCH_GIT_BASIC_AUTHORIZATION` başlığını kendisi üretir. Onu ayrıca secret olarak eklemeniz gerekmez.

### İsteğe bağlı variable (secret değil)

Aynı sayfada **Variables** sekmesi:

| Variable | Anlamı |
|---|---|
| `IOS_BUNDLE_ID` | Bundle id. Boşsa `com.rugtopia.game` kullanılır. |
| `MATCH_READONLY` | İlk başarılı yüklemeden sonra `true` yapın. O zaman CI yeni sertifika üretmez, sadece indirir. |

Elle TestFlight çalıştırırken formdaki bundle id alanı doluysa variable'ı ezer.

---

## 1. Unity Personal lisansını secret yapmak

Eski "manuel aktivasyon" sayfası (`license.unity3d.com`, `.alf` yükleme) **Personal lisans için kapalı**. O yolu izlemeyin. GameCI'nin anlattığı yol: Unity Hub ile `.ulf` üretmek.

GameCI, `UNITY_PASSWORD` içinde özel karakter olunca aktivasyonun bozulabildiğini yazıyor. Şifrenizde harf ve rakam dışında karakter varsa, secret'ı eklemeden önce Unity şifresini yalnızca harf ve rakamdan oluşacak şekilde değiştirin.

### 1.1 Hub ve lisans

1. https://unity.com/download adresinden **Unity Hub**'ı indirin (Windows veya Linux). Bu projeyi Hub'da açmanız gerekmez.
2. CI'de kullanacağınız Unity hesabıyla Hub'a girin.
3. **Unity Hub → Preferences → Licenses → Add → Get a free personal license.**
4. Listede lisans görünse bile `.ulf` oluşmamış olabilir. **Add** adımını atlamayın.

### 1.2 Dosyayı bulun

- Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
- Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`

`ProgramData` gizlidir ve okumak için yönetici izni gerekebilir. Dosya yoksa Hub'da Add adımını tekrarlayın. Bilgisayarda `Unity_lic.ulf` adını aratın.

Dosya XML'dir. İlk satırı `<?xml` ile başlar. Başını veya sonunu kesmeyin, tırnak içine almayın, base64 yapmayın.

Windows PowerShell (gerekirse "Yönetici olarak çalıştır"):

```powershell
Get-Content -Raw "C:\ProgramData\Unity\Unity_lic.ulf" | Set-Clipboard
```

### 1.3 GitHub'a yapıştırın

1. https://github.com/keremsek99-wq/rugtopia/settings/secrets/actions
2. **New repository secret**
3. Name: `UNITY_LICENSE` — Value: panodaki XML. **Add secret**
4. Aynı yerden `UNITY_EMAIL` ve `UNITY_PASSWORD`

Pro lisansınız varsa bu üçlüyü eklemeyin. Onun yerine `UNITY_SERIAL`, `UNITY_EMAIL`, `UNITY_PASSWORD` ekleyin. Seri numarası Unity hesap sayfanızdaki abonelikten gelir.

Secret'lar geldikten sonra `unity-build` workflow'u bir sonraki pull request veya `main` push'unda EditMode testini çalıştırır ve iOS Xcode projesini artifact olarak yükler. Bu, lisans eklenene kadar doğrulanmış değil.

---

## 2. Apple Developer üyeliği ve Team ID

TestFlight, ücretli **Apple Developer Program** ister (yıllık 99 USD). Kayıt tarayıcıdan yapılır: https://developer.apple.com/programs/enroll/ — Mac gerekmez.

1. https://developer.apple.com/account adresine girin.
2. **Membership details** bölümündeki **Team ID**'yi kopyalayın (10 karakter).
3. Bunu daha sonra `APPLE_TEAM_ID` secret'ı yapacaksınız.

App Store Connect'te bekleyen sözleşme varsa kabul edin: https://appstoreconnect.apple.com/agreements. Sözleşme kabul edilmeden yükleme reddedilir. Bu da tarayıcıdan yapılır.

---

## 3. Bundle id kaydı

Varsayılan: `com.rugtopia.game`. Başkası aldıysa kendinizinkini seçin (ör. `com.kerem.rugtopia`) ve onu `IOS_BUNDLE_ID` variable'ına yazın. Aşağıdaki adımlarda hep aynı id'yi kullanın.

1. https://developer.apple.com/account/resources/identifiers/list
2. **+** → **App IDs** → **App** → Continue
3. Description: `Rugtopia`
4. Bundle ID: **Explicit**, değer `com.rugtopia.game` (veya sizin seçtiğiniz)
5. Capability eklemeyin. **Register**

Match, id yoksa ilk TestFlight koşusunda kendisi de oluşturmayı dener. Yine de kaydı şimdi yapmak, App Store Connect uygulama formunda id'nin listede görünmesini sağlar.

---

## 4. App Store Connect'te uygulama kaydı

Apple'ın API'si yeni bir uygulama kaydı açmıyor. Bunu site üzerinden bir kez siz oluşturursunuz. Mac gerekmez.

1. https://appstoreconnect.apple.com/apps
2. **+** → **New App**
3. Platform: **iOS**
4. Name: `Rugtopia` (App Store'da görünür ad; doluysa sonuna bir kelime ekleyin)
5. Primary language: Turkish veya English
6. Bundle ID: 3. adımda kaydettiğiniz id
7. SKU: `rugtopia` (kullanıcının görmediği, benzersiz bir kod)
8. **Create**

Kayıt yoksa TestFlight yüklemesi "uygulama bulunamadı" diye düşer.

---

## 5. App Store Connect API anahtarı

fastlane, Apple ID şifresi ve SMS kodu istemesin diye bu anahtarı kullanır. Sertifika oluşturabildiği için rol **Admin** olmalı. App Manager yetmez.

1. https://appstoreconnect.apple.com/access/integrations/api
   (Users and Access → Integrations → App Store Connect API)
2. **Team Keys** bölümünde **Generate API Key** (Individual key değil).
3. Name: `Rugtopia CI`. Access: **Admin**. Generate.
4. Sayfanın üstündeki **Issuer ID**'yi kopyalayın → secret `APPSTORE_ISSUER_ID`
5. Satırdaki **Key ID**'yi kopyalayın → secret `APPSTORE_KEY_ID`
6. **Download** ile `.p8` dosyasını indirin. Apple bu dosyayı bir kez verir. Kaybederseniz yeni anahtar üretmeniz gerekir.

`.p8` dosyasını base64 yapın. Çıktı tek satır olmalı. PowerShell:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("$env:USERPROFILE\Downloads\AuthKey_XXXXXXXXXX.p8"))
```

`XXXXXXXXXX` yerine Key ID gelir. Çıktının tamamını kopyalayıp `APPSTORE_P8` secret'ına yapıştırın. Tırnak eklemeyin, satır kırmayın.

`.p8` dosyasını repoya, e-postaya veya bu sohbete koymayın. İndirilenler klasöründe sizde dursun.

---

## 6. Sertifikalar için private repo (match)

İmza sertifikası bu public oyunun reposuna **giremez**. Match onu ayrı bir private repoda, sizin seçtiğiniz bir parolayla şifreler.

### 6.1 Repoyu açın

1. https://github.com/new
2. Name: `rugtopia-certificates`
3. **Private**
4. README, .gitignore ve lisans **eklemeyin** (boş repo). Create.

Secret `MATCH_GIT_URL` değeri:

```text
https://github.com/keremsek99-wq/rugtopia-certificates.git
```

Workflow, bu adres `https://github.com/keremsek99-wq/rugtopia` ise durur. Sertifikayı yanlışlıkla public repoya yazmasın diye.

### 6.2 Parola

`MATCH_PASSWORD` uzun ve rastgele olsun. Bunu unutursanız repodaki sertifikalar açılamaz; yenilerini üretmek gerekir. Bir şifre yöneticisine yazın.

PowerShell örneği:

```powershell
-join ((48..57 + 65..90 + 97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

### 6.3 Token

Fine-grained personal access token, yalnızca sertifika reposuna yazsın. Oyun reposuna yetki vermeyin.

1. https://github.com/settings/personal-access-tokens/new
2. Token name: `rugtopia-match`
3. Resource owner: kendi kullanıcınız
4. Repository access: **Only select repositories** → `rugtopia-certificates`
5. Permissions → Repository permissions → **Contents: Read and write**
6. Generate. `github_pat_...` değerini bir kez görürsünüz. Bu `MATCH_GIT_TOKEN` secret'ıdır.

---

## 7. Secret'ları GitHub'a ekleyin

https://github.com/keremsek99-wq/rugtopia/settings/secrets/actions

Her biri için **New repository secret**. Name kutusuna aşağıdaki isim, Value kutusuna değer.

Unity:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Apple ve match:

- `APPSTORE_KEY_ID`
- `APPSTORE_ISSUER_ID`
- `APPSTORE_P8`
- `APPLE_TEAM_ID`
- `MATCH_GIT_URL`
- `MATCH_PASSWORD`
- `MATCH_GIT_TOKEN`

İsteğe bağlı, **Variables** sekmesi (secret değil):

- `IOS_BUNDLE_ID` = seçtiğiniz bundle id (varsayılanı kullanacaksanız boş bırakın)
- `MATCH_READONLY` şimdilik eklemeyin

Secret değerini sonradan kimse okuyamaz; yalnızca üzerine yazılır. Yanlış yapıştırdıysanız aynı isimle yeniden oluşturun.

---

## 8. İlk TestFlight

1. Bu pull request `main`'e girdikten sonra repoda **Actions → ios-testflight → Run workflow**.
2. Branch: `main`. Bundle id boş kalabilir.
3. İlk koşu uzun sürer: Unity iOS export, sonra macOS'ta imza. Runner sertifikayı oluşturup private repoya yazar, IPA'yı TestFlight'a yükler.
4. İş bitince App Store Connect → uygulamanız → TestFlight'ta build "Processing" olarak görünür. Workflow, Apple'ın işlemesini beklemeden çıkar (`skip_waiting_for_build_processing`). Birkaç dakika sonra build hazır olur.
5. İşe yaradığını görünce Variables'a `MATCH_READONLY` = `true` ekleyin. Sonraki koşular yeni sertifika açmaz.

Sürüm etiketi de aynı workflow'u başlatır. GitHub → **Releases → Draft a new release**. Tag: `v0.1.0` ("Choose a tag" kutusuna yazıp **Create new tag**). Target: `main`. Publish.

Etiket `v1.2.3` ise mağaza sürümü `1.2.3` olur. Etiketin `v` harfinden sonrası sürüm metnidir; `v0.1.0` gibi düz bir sürüm kullanın. Etiketsiz elle çalıştırmada sürüm `0.1.0` kalır. Build numarası her koşuda UTC zaman damgasıdır; TestFlight aynı numarayı ikinci kez kabul etmez.

---

## 9. Secret yokken ne görmelisiniz

Actions sekmesinde:

- `core-tests` yeşil, test sayısı 260.
- `unity-build` yeşil. Adımın özetinde **Unity build skipped** ve hangi secret grubunun eksik olduğu yazar. Kırmızı değildir.
- `ios-testflight` elle çalıştırılınca yeşil. Özetinde **TestFlight skipped** yazar. Unity derlemesi ve macOS runner başlamaz.

Fork'tan açılan pull request'lere GitHub secret vermez. O request'lerde Unity işi de bu yüzden atlanır. Bu beklenen davranıştır.

---

## Takılırsa

- **Unity aktivasyonu düşüyor.** Şifrede özel karakter olmasın. `UNITY_LICENSE` dosyanın ham XML'i olsun, base64 olmasın. Personal ile `UNITY_SERIAL`'ı aynı anda doldurmayın.
- **`.ulf` yok.** Hub → Licenses → Add → Get a free personal license. Listede lisans görünmesi dosyanın yazıldığı anlamına gelmez.
- **Sertifika oluşmuyor.** API anahtarı Team Key ve rolü Admin olsun. `MATCH_GIT_TOKEN` yalnızca private sertifika reposunda Contents: Read and write taşısın. `MATCH_GIT_URL` HTTPS olsun ve sonu `.git` olabilir.
- **TestFlight "app not found".** 4. adımdaki uygulama kaydı, bundle id ile aynı olsun.
- **Sözleşme hatası.** App Store Connect → Agreements. Bekleyen sözleşmeyi tarayıcıdan kabul edin.
- **İkinci yükleme "build already exists".** Yeni bir workflow run başlatın. Aynı run'ı yeniden koşturmak aynı zaman damgasını kullanmaz; her yeni başlatma yeni numara üretir.
