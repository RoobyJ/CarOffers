using System;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.IO;
using test_exercise_otomoto;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

public class Test
{
    public static void Main(string[] args)
    {
        // variables
        List<string> offers = new List<string>();
        HtmlNodeCollection? nodes;
        string[] names = new string[] { "producent", "model", "silnik", "generacja", };
        string? path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        string html = "https://www.otomoto.pl/osobowe";
        long start = getTime();
        HtmlWeb web = new();

        //Connecting to the MySql server
        var db_conn = SQL_DATABASE.Instance();
        db_conn.Server = "localhost";
        db_conn.DatabaseName = "mydb";
        db_conn.UserName = "root";
        db_conn.Password = "";


        // this code get's us the html of the site
        // the while loop is to emilinate chance to get null response
        while (true)
        {
            long end = getTime();
            if (end - start > 2000)
            {
                nodes = web.Load(html).DocumentNode.SelectNodes("/html/body");
                if (nodes != null)
                {
                    Console.WriteLine("#Got start page");
                    break;
                }
            }

        }

        // here we look for links for the offers and put them into a list
        foreach (var node in nodes)
        {

            foreach (HtmlNode link in node.SelectNodes("//a[@href]"))
            {

                string hrefValue = link.GetAttributeValue("href", string.Empty);

                if (hrefValue.Trim().StartsWith("https://www.otomoto.pl/oferta/") & !offers.Contains(hrefValue))
                {
                    offers.Add(hrefValue);
                }

            }
        }


        // here open each offer separate
        foreach (string offer_html in offers)
        {
            

            start = getTime();

            // again what above
            while (true)
            {
                Dictionary<string, string> car_parameters = new Dictionary<string, string>(); // this dict is to store collected data of current offer
                long end = getTime();

                if (end - start > 2000)
                {

                    Console.WriteLine("#Entering offer");
                    Console.WriteLine();
                    Console.WriteLine(offer_html);
                    var doc = web.Load(offer_html);

                    IEnumerable<HtmlNode> id = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("offer-meta__value")); // id value
                    string offer_id = id.ElementAt(1).InnerText;
                    car_parameters.Add("offer_id", id.ElementAt(1).InnerText);
                    if (Directory.Exists(path + offer_id))
                    {
                        Console.WriteLine("This offer already exists");
                        break;
                    }
                    else
                    {
                        IEnumerable<HtmlNode> param_nodes = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("offer-params__list"));   // html code with car parameters

                        IEnumerable<HtmlNode> img_nodes = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("offer-content__gallery")); // html code with images
                        var urls = img_nodes.ElementAt(0).Descendants("img")
                                    .Select(e => e.GetAttributeValue("src", null))
                                    .Where(s => !String.IsNullOrEmpty(s));

                        //Creating directory for photos
                        Directory.CreateDirectory(path + id.ElementAt(1).InnerText);

                        //Downloading photos to dir by offer_id
                        download_images(urls, path, offer_id);


                        foreach (HtmlNode node in param_nodes)
                        {


                            var nodes_param_name_list = node.Descendants(0).Where(n => n.HasClass("offer-params__label"));
                            var nodes_param_list = node.Descendants(0).Where(n => n.HasClass("offer-params__value"));

                            int ctn1 = 0;
                            foreach (HtmlNode _node in nodes_param_name_list)
                            {
                                string name_of_label = _node.InnerText.ToLower();
                                if (name_of_label == "przebieg")
                                {
                                    car_parameters.Add(name_of_label, ReplaceWhitespace(nodes_param_list.ElementAt(ctn1).InnerText, ""));
                                }
                                else if (name_of_label == "marka pojazdu")
                                {
                                    car_parameters.Add(name_of_label, ReplaceWhitespace(nodes_param_list.ElementAt(ctn1).InnerText, ""));
                                }
                                else if (name_of_label == "model pojazdu")
                                {
                                    car_parameters.Add(name_of_label, ReplaceWhitespace(nodes_param_list.ElementAt(ctn1).InnerText, ""));
                                }
                                else if (name_of_label == "rok produkcji")
                                {
                                    car_parameters.Add(name_of_label, ReplaceWhitespace(nodes_param_list.ElementAt(ctn1).InnerText, ""));
                                }
                                else if (name_of_label == "rodzaj paliwa")
                                {
                                    car_parameters.Add(name_of_label, ReplaceWhitespace(nodes_param_list.ElementAt(ctn1).InnerText, ""));
                                }
                                else if (name_of_label == "moc")
                                {
                                    car_parameters.Add(name_of_label, ReplaceWhitespace(nodes_param_list.ElementAt(ctn1).InnerText, ""));
                                }
                                ctn1++;
                            }
                        }
                        
                        if (db_conn.IsConnect())
                        {
                            List<string> _keys = car_parameters.Keys.ToList();
                            List<string> _values = car_parameters.Values.ToList();
                            
                            // create brand table
                            db_conn.CreateTable(_values.ElementAt(1), _keys.ElementAt(0), _keys.ElementAt(1).Replace(" ", "_"), _keys.ElementAt(2).Replace(" ", "_"), _keys.ElementAt(3).Replace(" ", "_"), _keys.ElementAt(4), _keys.ElementAt(5).Replace(" ", "_"), _keys.ElementAt(6));
                            // create model table
                            db_conn.CreateTable(_values.ElementAt(2), _keys.ElementAt(0), _keys.ElementAt(1).Replace(" ", "_"), _keys.ElementAt(2).Replace(" ", "_"), _keys.ElementAt(3).Replace(" ", "_"), _keys.ElementAt(4), _keys.ElementAt(5).Replace(" ", "_"), _keys.ElementAt(6));
                            // insert data to brand table
                            db_conn.PostData(_values.ElementAt(1), _values.ElementAt(0), _values.ElementAt(1), _values.ElementAt(2), _values.ElementAt(3), _values.ElementAt(4), _values.ElementAt(5), _values.ElementAt(6));
                            // insert data to model table
                            db_conn.PostData(_values.ElementAt(2), _values.ElementAt(0), _values.ElementAt(1), _values.ElementAt(2), _values.ElementAt(3), _values.ElementAt(4), _values.ElementAt(5), _values.ElementAt(6));
                            



                        }

                        // If the task ends succesfull or exists already go to next offer
                        if (nodes != null || Directory.Exists(path + offer_id))
                        {
                            
                            break;
                        }
                    }

                }
                // otwarcie sql
                // sprawdzenie czy jest taki ID
                // jezeli nie to dodaj nowy rekord inaczej pomin
                // 
            }
        }
        db_conn.Close();
    }

    private static DateTime JanFirst1970 = new DateTime(1970, 1, 1);
    public static long getTime()
    {
        return (long)((DateTime.Now.ToUniversalTime() - JanFirst1970).TotalMilliseconds + 0.5);
    }

    private static int ctn = 0;
    public static void download_images(IEnumerable<string> urls, string path, string id)
    {
        foreach (var url in urls)
        {
            WebClient client = new WebClient();
            string _url = url.Remove(url.Count() - 7) + "1080x720";
            //Console.WriteLine(_url);
            client.DownloadFile(new Uri(_url), String.Format(@"{0}{1}\{2}.jpg", path, id, ctn.ToString()));
            ctn++;
        }
    }
    private static readonly Regex sWhitespace = new Regex(@"\s+");
    public static string ReplaceWhitespace(string input, string replacement)
    {
        return sWhitespace.Replace(input, replacement);
    }
}

