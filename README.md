<div align="center">
  <br/>
  <img src="https://raw.githubusercontent.com/muhammet-kandemir-95/dmuka2.CS.NeoCache/master/mdcontent/images/main.png" />
  <br/>
  <img width="126px" src="https://raw.githubusercontent.com/muhammet-kandemir-95/dmuka2.CS.NeoCache/master/mdcontent/images/version.png" alt="version" />
  <br/>
  <h2>VERSION 1.0.0.0</h2>
</div>

> **Version Schema**
> 
> "_To Change the Algorithms of Programs_"."_To Add New Command_"."_To Fix Important Bug_"."_To Fix Bug or To Improve Some Features_"

# NeoCache Nedir?

 RAM üzerinden çalışan ilişkisel veri tabanı. Disk üzerinde herhangi bir işlem yapmamaktadır. _REDIS_ sisteminden en önemli farkı beyindeki nöronlar gibi ilişkisel bir yapıya sahip olması diyebiliriz.

 Aynı zamanda _C# Core_ ile verilerin yönetimi çok rahat bir şekilde yapılabilmektedir. Örnek verecek olursak bir verinin değiştirilmesi veya okunması durumunda herhangi bir dizi kodu tetikleyebiliriz. Aynı zamanda sistemin ilk açılmasında başka bir veri tabanına bağlanarak _VARNISH_ gibi bir ara katman oluşturabiliriz.

 **NeoCache** ile alakalı bilinmesi gereken en önemli kurallardan biri sunucu tarafındaki başlangıç işlemleridir. Aşağıda da anlatılacağı üzere sunucunun göreceği talep ve entegre olacağı sistemin çalışma mimarisine göre tasarlanması çok önemlidir. Çünkü **NeoCache** aynı _REDIS_ gibi kullanılabileceği gibi aynı zamanda _model_ tabanlı ilişkili bir _cache_ sistemi olarakta kullanılabilir. Hatta veri tabanları ile entegre edilip çok daha kapsamlı hale getirilebilir. Fakat bunların hepsinin getireceği bazı sorumluluklar olacaktır. İşte bu noktada dökümanın ilerideki kısımlarınıda okuduktan sonra karar vermeniz çok daha sağlıklı olacaktır.

 # Nasıl bir sunucu kurabilirim? Neler gerekli?

 Aslında **NeoCache** tek başına çalışabilen bir servis değildir. Bir kütüphane gibi düşünebiliriz. Öncelikle _dotnet core_ teknolojisini kullanmanız gerekir. Projenize referans olarak ekledikten sonra belli kodları ekleyerek sistemi kurabilirsiniz. Aslında bir ham madde gibi düşünülebilir bir nebze.

```csharp
BrainServer server = new BrainServer(1/* Core Count */, "mysecretpass"/* Password */, 1234/* Port */);
server.Open();
```

 Yukarıda görüldüğü üzere aslında oldukça basit bir şekilde sunucu kurabilirsiniz. Fakat malesef iş bu kadar basit değil. En önemli kısım yani modellere ihtiyacımız var. Mevcut proje içerisinde doğrudan veya referans olarak bulunması gereken modellerimize örnek olarak aşağıdaki kodları inceleyebiliriz.

> Eğer sunucunun _SSL_ ile çalışmasını istiyorsak _OpenWithSSL_ metodunu kullanabiliriz.
> Hepsinden önce projenizde _unsafe_ kod derlenme özelliğini açmanız gerekmektedir!

## Sınıf Uygulaması

<div>
  <img src="https://github.com/muhammet-kandemir-95/dmuka2.CS.NeoCache/blob/master/mdcontent/images/schoolexample.gif?raw=true" />
</div>

 Uygulamamızda bir sınıf, sınıfa ait bir öğretmen ve öğrenciler olacak. Her öğrencinin sınav sonuçları olacak. Hadi başlayalım.

> Projede yer alan _dmuka2.CS.NeoCache.TestApp.Server_ ve _dmuka2.CS.NeoCache.TestApp.Admin_ uygulamarını çalıştırarak canlı olarak veriyi inceleyebilir, test edebilirsiniz.

#### Exam

```csharp
public class Exam : dmuka2.CS.NeoCache.Neuron<Exam>
{
    [NeuronData]
    public string name = "";
    [NeuronData]
    public byte result = 0;
    public void result__Pointer(Action<IntPtr> callback, bool set)
    {
        unsafe
        {
            fixed (byte* address = &result)
            {
                callback((IntPtr)address);
            }
        }
    }
}
```

1. İlk modelimiz sınav olacak. Burada dikkatimizi çeken kısım "*result__Pointer*" olmalı. Aslında projenin kalbi diyebileceğimiz kısımda tam olarak burası. Eğer verimiz _byte_, _sbyte_, _short_, _ushort_, _int_, _uint_, _long_, _ulong_, _float_, _double_ veya _decimal_ ise o değişkenin adına "*__Pointer*" eklenmiş halde yukarıdaki gibi bir metot eklenmelidir. Bu metot değer doldurulurken veya çekilirken verinin adresini bulmak için kullanılacaktır. Yani burada verinin gelip gitmesi sırasında manuel müdahale edilebilir. Eğer _set_ değeri _true_ ise değer ataması, değil ise değer okunuyor demektir.

> Burada _field_ kullanmak yerine _property_ kullanılabilir. Fakat adres işaretlenmesi işlemlerinde _property_ üzerinde _get_ ve _set_ kısımları tetiklenmeyecektir.
> Sadece _byte_ veri türü için _get_ tetiklenecektir. Onun dışındaki adres işaretlemesi gereken verilerde _property_ kullanmak gereksizdir.

2. _String_ veri türü için herhangi bir adres metotuna gerek yoktur.

3. Her verinin _NeuronDataAttribute_ bilgisi olma zorunluluğu vardır. Aksi takdirde bu veriye bağlantı oluşturulmaz.

#### Student

```csharp
public class Student : dmuka2.CS.NeoCache.Neuron<Student>
{
    [NeuronData]
    public string name = "";
    [NeuronData]
    public string surname = "";
    [NeuronData]
    public short birth_year = 0;
    public void birth_year__Pointer(Action<IntPtr> callback, bool set)
    {
        unsafe
        {
            fixed (short* address = &birth_year)
            {
                callback((IntPtr)address);
            }
        }
    }
    [NeuronData]
    [NeuronList(MaxRowCount = 5)]
    public List<Exam> exams = new List<Exam>();
}
```

1. İkinci modelimiz öğrenci olacak. Önceki modelden farklı olarak bir _IList_ verisi ve _NeuoronListAttribute_ verisi bulunmaktadır. Burada bir verinin birden fazla veriyle ilişkide olabilmesini sağlamaktadır.

2. _MaxRowCount_ özelliği oldukça fazla önemlidir. Çünkü verinin maksimum ulaşılabileceği adeti temsil etmektedir. Tabikide _dotnet core_ mimarisinde kapasite buradakindan çok daha fazladır. Fakat **NeoCache** sistemi verilerin kendisine ulaşmak için başlangıçta tüm yolları oluşturmaktadır. Yani programın başladıktan sonra yeni bir veri yolu eklenmez. Bu durumda bir _IList_ değeri kullanıldığı takdirde maksimum kapasitesine göre yol haritası çıkarmak oldukça büyük bir maliyet olabilir. O yüzden bunu mimariyi tasarlayan yazılımcının modelleme aşamasında minimum seviyede tutması gerekmektedir. Aksi takdirde 2 katmanlı bir modelde 1.000.000 üssü 1.000.000 gibi bir kombinasyon ortaya çıkabilir.

#### Teacher

```csharp
public class Teacher : dmuka2.CS.NeoCache.Neuron<Teacher>
{
    [NeuronData]
    public string name = "";
    [NeuronData]
    public string surname = "";
    [NeuronData]
    public short birth_year = 0;
    public void birth_year__Pointer(Action<IntPtr> callback, bool set)
    {
        unsafe
        {
            fixed (short* address = &birth_year)
            {
                callback((IntPtr)address);
            }
        }
    }
}
```

1. Üçüncü modelimiz öğretmen olacak. Burada aslında önceki modellerden farklı hiçbir şey kullanılmamıştır.


#### Class

```csharp
public class Class : dmuka2.CS.NeoCache.Neuron<Class>
{
    [NeuronData]
    public string name = "";
    [NeuronData]
    public Teacher teacher = new Teacher();
    [NeuronData]
    [NeuronList(MaxRowCount = 5)]
    public List<Student> students = new List<Student>();
}
```

1. Dördüncü ve son modelimiz sınıf olacaktır. Burada başka bir _neuron_ ile ilişki kurmanın 2 farklı kuralıda yer almaktadır. _IList_ durumunu yukarıda anlatmıştık. Farklı olarak bire bir ilişki durumu karşımızda. Aslında çok bir fark bulunmamaktadır. Direk sınıfı buraya yazarak ilişki kurabiliriz.

2. _Property_ kullanarak _teacher_ değerinin doldurulmasını ve kullanılmasını durumlarını yönetebiliriz.

### Client Uygulaması Kullanarak Verileri Almak

 Yukarıdaki modelleri kullanarak **NeoCache** sunucusuna bağlantı yapmanın kodlarını görüceğiz.

 #### Sunucu Bağlantısı Açmak / Kapatmak

 ```csharp
// Create a connector.
BrainClient client = new BrainClient();

// Open the connection without ssl.
client.Open("localhost", port, pass);

// Open the connection with ssl.
client.Open("localhost", port, pass, ssl: true, sslTargetHost: "muhammetkandemir.com");

/* Codes ... */

// Close the connection.
client.Close();
 ```

#### Yeni Bir Neuron Oluşturmak

```csharp
client.AddANeuron("class_A-1"/* Neuron ID */, "Class" /* Class Name by Models = Class, Teacher, Student, Exam for this example */);
```

#### Mevcut Bir Neuron'a Değer Atamak / Değer Almak

```csharp
client.SetNeuronValue("class_A-1"/* Neuron ID */, "name" /* Variable Name / Path */, "A-1" /* Value */);

var valString = client.GetNeuronValueAsString("any_neuron_id"/* Neuron Id */, "stringValue"/* Variable Name / Path */, "abc"/* Value */);
var valByte = client.GetNeuronValueAsByte("any_neuron_id"/* Neuron Id */, "byteValue"/* Variable Name / Path */, (byte)55/* Value */);
var valSByte = client.GetNeuronValueAsSByte("any_neuron_id"/* Neuron Id */, "sbyteValue"/* Variable Name / Path */, (sbyte)55/* Value */);
var valInt16 = client.GetNeuronValueAsInt16("any_neuron_id"/* Neuron Id */, "shortValue"/* Variable Name / Path */, (short)55/* Value */);
var valUInt16 = client.GetNeuronValueAsUInt16("any_neuron_id"/* Neuron Id */, "ushortValue"/* Variable Name / Path */, (ushort)55/* Value */);
var valInt32 = client.GetNeuronValueAsInt32("any_neuron_id"/* Neuron Id */, "intValue"/* Variable Name / Path */, (int)55/* Value */);
var valUInt32 = client.GetNeuronValueAsUInt32("any_neuron_id"/* Neuron Id */, "uintValue"/* Variable Name / Path */, (uint)55/* Value */);
var valInt64 = client.GetNeuronValueAsInt64("any_neuron_id"/* Neuron Id */, "longValue"/* Variable Name / Path */, (long)55/* Value */);
var valUInt64 = client.GetNeuronValueAsUInt64("any_neuron_id"/* Neuron Id */, "ulongValue"/* Variable Name / Path */, (ulong)55/* Value */);
var valSingle = client.GetNeuronValueAsSingle("any_neuron_id"/* Neuron Id */, "floatValue"/* Variable Name / Path */, (float)55/* Value */);
var valDouble = client.GetNeuronValueAsDouble("any_neuron_id"/* Neuron Id */, "doubleValue"/* Variable Name / Path */, (double)55/* Value */);
var valDecimal = client.GetNeuronValueAsDecimal("any_neuron_id"/* Neuron Id */, "decimalValue"/* Variable Name / Path */, (decimal)55/* Value */);
```

> **NeoCache** tamamiyle _byte[]_ ile haberleşmesinden dolayı veriler ağdaki ham haliyle gelmektedir. Dönüşümler **NeoCache** içerisindeki hazır metotlar aracılığıyla yapılabilir.
> _SetNeuronValue_ değeri hassas bir metotdur. Modeldeki veri türü ne ise burada o türde gitmelidir. _byte_ veri türü için _int_ değer gönderilmemelidir!
> Aynı kural _GetNeuronValue_ için de geçerlidir.

#### Bire - Bir İlişkideki Bir Neuron'a Değer Atamak / Değer Almak

```csharp
// Class (A-1) - Teacher (Medine KANDEMIR)
client.SetNeuronValue("class_A-1"/* Neuron ID */, "teacher.name", "Medine");
client.SetNeuronValue("class_A-1"/* Neuron ID */, "teacher.surname", "KANDEMIR");
client.SetNeuronValue("class_A-1"/* Neuron ID */, "teacher.birth_year", (short)1989);
```

#### Bire - Çok İlişkideki Bir Neorun'a Değer Eklemek / Değer Atamak / Değer Almak

```csharp
// Class (A-1) - Student[0] (Muhammet KANDEMIR)
// We are adding new instance to list.
client.AddANeuronToList("class_A-1", "students");
// We are setting new values to the list item.
client.SetNeuronValue("class_A-1", "students[0].name", "Muhammet");
client.SetNeuronValue("class_A-1", "students[0].surname", "KANDEMIR");
client.SetNeuronValue("class_A-1", "students[0].birth_year", (short)1995);

// Class (A-1) - Student[1] (Nesibe KANDEMIR)
// We are adding new instance to list.
client.AddANeuronToList("class_A-1", "students");
// We are setting new values to the list item.
client.SetNeuronValue("class_A-1", "students[1].name", "Nesibe");
client.SetNeuronValue("class_A-1", "students[1].surname", "KANDEMIR");
client.SetNeuronValue("class_A-1", "students[1].birth_year", (short)1996);

// Class (A-1) - Student[2] (Omer KANDEMIR)
// We are adding new instance to list.
client.AddANeuronToList("class_A-1", "students");
// We are setting new values to the list item.
client.SetNeuronValue("class_A-1", "students[2].name", "Omer");
client.SetNeuronValue("class_A-1", "students[2].surname", "KANDEMIR");
client.SetNeuronValue("class_A-1", "students[2].birth_year", (short)1995);
```