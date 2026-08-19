Console.WriteLine("Hello, World!");

Console.Write("hola mundo"); //Soy un comentario
Console.Write("Hola mundo 2");

/*
y un bloque de comentarios
*/
Console.WriteLine("");
Console.WriteLine("Ingrese su nombre");
String? nombre =Console.ReadLine();
Console.WriteLine("Su nombre es: "+nombre);
int edad=32, peso=100;
float estatura=1.67f;
double promedio=1.67;
bool estaViva=true;
char letra='c';
Console.WriteLine("Ingrese edad: ");
edad=int.Parse(Console.ReadLine()!);
Console.WriteLine("El proximo año tendra: " +(edad+1));

int a=23:
double aa=2.3;
int bb=(int)aa;
string h=aa.ToString("f3");//convertir en string
Console.WriteLine("h="+h);
bool estaVivo=int.TryParse("3.4",out int zz);
Console.WriteLine("zz)="zz);