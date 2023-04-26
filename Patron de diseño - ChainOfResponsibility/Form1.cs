using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Patron_de_diseño___ChainOfResponsibility
{
    // Validation app. Validar User Passwors and Role

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            db = new Database();
            db.agregarCliente(new Cliente("Chiara","1234","admin"));
            db.agregarCliente(new Cliente("DanielK", "hola", "user"));
            db.agregarCliente(new Cliente("Valentina", "passw", "admin"));
            db.agregarCliente(new Cliente("Bianca", "hola", "user"));
        }
        Database db;
        private void Form1_Load(object sender, EventArgs e)
        {
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Handler handler = new UserExistsHandler(db);
            handler.next = new ValidPasswordHandler(db);
            handler.next.next = new RolCheckHandler();
            
            AuthService service = new AuthService(handler);
            service.logIn(textBox1.Text, textBox2.Text, textBox3.Text);
        }
    }
    public class Cliente
    {
        public string Usuario, Contraseña, Rol;
        public Cliente(string pUsuario, string pContraseña, string pRol)
        {
            Usuario = pUsuario;
            Contraseña = pContraseña;
            Rol = pRol;
        }
        public Cliente(string pUsuario, string pContraseña)
        {
            Usuario = pUsuario;
            Contraseña = pContraseña;
        }
    }

    public class Database
    {
        //contiene los datos de usuario, contraseña, rol de los usuarios
        List<Cliente> lc;

        public Database()
        {
            lc = new List<Cliente>();
            
        }
        public void agregarCliente(Cliente cliente)
        {
            lc.Add(cliente);
        }

        public Boolean isValidUser(string username)
        {
            //chequea que el usuario exista en la lista de usuarios

            int indice = lc.FindIndex(x => x.Usuario == username);
            if (indice != -1)
            {
                return true;
            }
            return false;
            
        }
        public Boolean isValidPassword(string username, string password) 
        {


            int indice = lc.FindIndex(x => x.Usuario == username);
            if (indice != -1)
            {
                if (lc[indice].Contraseña == password)
                return true;
            }
            return false;

        }
    }

    
    public abstract class Handler
    {
        public Handler next;//guarda referencia al siguiente handler de la cadena
        

        public abstract bool handle(Cliente cliente);// metodo abstracto handler que va a implementar por el concrete handler
        
        protected bool handleNext(Cliente cliente)
        {
            //pasar request al siguiente objeto a menos que ya no haya
            if(next == null)
            {
                return true;
            }
            return next.handle(cliente);
        }
    }



    //*****Manejadores concretos*****
    public class UserExistsHandler : Handler
    {
        private Database database;
        public UserExistsHandler(Database database)
        {
            this.database = database;
        }

        public override bool handle(Cliente cliente)
        {
            if (!database.isValidUser(cliente.Usuario))
            {
                //Console.WriteLine("El usuario no existe");
                MessageBox.Show("El usuario no existe");
                return false;
            }
            return handleNext(cliente);
        }
    }
    public class ValidPasswordHandler : Handler
    {
        private Database database;
        public ValidPasswordHandler(Database database)
        {
            this.database = database;
        }

        public override bool handle(Cliente cliente)
        {
            if (!database.isValidPassword(cliente.Usuario,cliente.Contraseña))
            {
                //Console.WriteLine("Incorrect password");
                MessageBox.Show("Contraseña incorrecta");
                return false;
            }
            return handleNext(cliente);
        }
    }
    public class RolCheckHandler : Handler
    {
        public override bool handle(Cliente cliente)
        {
            if (cliente.Rol != "admin")
            {
                //Console.WriteLine("Cliente no tiene permisos de administrador");
                MessageBox.Show("El cliente no tiene permisos de administrador");

                return false;
            }
            
            //Console.Write("Loading Default Page...");
            MessageBox.Show("Authenticación correcta");
            //Console.WriteLine("Authenticación correcta");
            return handleNext(cliente);
        }
    }

    public class AuthService
    {
        private Handler handler;
        //este handler se usa en el login para invokar el metodo handle
        //en el handler rais de la cadena
        public AuthService(Handler handler)
        {
            this.handler = handler;
        }
        public bool logIn(string pUsuario, string pPassword, string rol)
        {
            bool authenticado = handler.handle(new Cliente(pUsuario, pPassword, rol));
            if (authenticado) 
            {
                //si  true el usuario paso todos los chequeos de los diferentes hanlders
                //de la cadena y fue autorizado adentro de la app
                Console.WriteLine("Authorization was successfull");
                return true;
            }
            return false;
        }
    }

}
