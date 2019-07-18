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

> Hepsinden önce projenizde _unsafe_ kod derlenme özelliğini açmanız gerekmektedir!

## Okul Uygulaması

<div>
  <img src="https://github.com/muhammet-kandemir-95/dmuka2.CS.NeoCache/blob/master/mdcontent/images/schoolexample.gif?raw=true" />
</div>

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

### Exam

1. İlk modelimiz sınav olacak. Burada dikkatimizi çeken kısım "*result__Pointer*" olmalı. Aslında projenin kalbi diyebileceğimiz kısımda tam olarak burası. Eğer verimiz _byte_, _sbyte_, _short_, _ushort_, _int_, _uint_, _long_, _ulong_, _float_, _double_ veya _decimal_ ise o değişkenin adına "*__Pointer*" eklenmiş halde yukarıdaki gibi bir metot eklenmelidir. Bu metot değer doldurulurken veya çekilirken verinin adresini bulmak için kullanılacaktır. Yani burada verinin gelip gitmesi sırasında manuel müdahale edilebilir. Eğer _set_ değeri _true_ ise değer ataması, değil ise değer okunuyor demektir.

> Burada _field_ kullanmak yerine _property_ kullanılabilir. Fakat adres işaretlenmesi işlemlerinde _property_ üzerinde _get_ ve _set_ kısımları tetiklenmeyecektir.
> Sadece _byte_ veri türü için _get_ tetiklenecektir. Onun dışındaki adres işaretlemesi gereken verilerde _property_ kullanmak gereksizdir.

2. _String_ veri türü için herhangi bir adres metotuna gerek yoktur.

3. Her verinin _NeuronDataAttribute_ bilgisi olma zorunluluğu vardır. Aksi takdirde bu veriye bağlantı oluşturulmaz.


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

### Student

1. İkinci modelimiz öğrenci olacak. Önceki modelden farklı olarak bir _IList_ verisi ve _NeuoronListAttribute_ verisi bulunmaktadır. Burada bir verinin 