using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace i2gawa.Core.Service
{
    public class HttpServices
    {
        private DataTable table = new DataTable();

        //Função que possobilita extrairmos as informações da tabela que quisermos.
        public DataTable Get(string query, IConfiguration _configuration)
        {
            //Aqui processamos a query
            Processing(query, _configuration);
            //aqui retornamos as informações em formato JSON
            return table;
        }

        //Função que possibilita inserir valores na tabela que quisermos.
        public string Post(string query, IConfiguration _configuration)
        {
            //Aqui processamos a query
            Processing(query, _configuration);
            //aqui retornamos um aviso de sucesso
            return ("Added Succesfully");
        }

        //Função que possibilita atualizar valores na tabela que quisermos.
        public string Put(string query, IConfiguration _configuration)
        {
            //Aqui processamos a query
            Processing(query, _configuration);
            //aqui retornamos um aviso de sucesso
            return ("Updated Succesfully");
        }

        //Função que possibilita deletar a item tabela que quisermos
        public string Delete(string query, IConfiguration _configuration)
        {
            //Aqui processamos a query
            Processing(query, _configuration);
            //aqui retornamos um aviso de sucesso
            return ("Deleted Succesfully");
        }

        public void Processing(string query, IConfiguration _configuration)
        {
            //Daqui pra baixo estamos criando a conexão com a tabela do banco de dados que queremos extrair as informações
            string sqlDataSource = _configuration.GetConnectionString("SqlConnection"); //EmployeeAppCon está definido no appsettings.json
            SqlConnection myCon = new SqlConnection(sqlDataSource);
            SqlCommand myCommand = new SqlCommand(query, myCon);

            SqlDataReader myReader;

            using (myCon)
            {
                myCon.Open();
                using (myCommand)
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

        }
    }
    }
