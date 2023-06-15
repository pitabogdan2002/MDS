# BestBooking (Proiect MDS)

Dănescu Adela-Gabriela, Dîrțu Ecaterina, Mihailescu Teodor, Pița Bogdan-Ioan 

grupa 242


## Epic story:

#### Booking.com este suprasolicitat. 
#### Pentru binele cumparatorilor, vrem sa ii ajutam sa gaseasca cele mai bune preturi intr-o noua aplicatie realizata de noi!


## User stories:

[Link Trello](https://trello.com/invite/mdsbestbooking/ATTI433e5f54374e851e879a648323e985b6BA271634)

[Link Demo](https://youtu.be/TXNqJDGsOgc)

[Link Diagrama](https://drive.google.com/file/d/18PXyDElgSDRy9ncKxb538-3kkto0WEC3/view?usp=sharing)

### MUST: (3p) logarea userilor cu diferite roluri  :heavy_check_mark:
- (1p) login standard 
- (2p) tipuri de useri si atributiile lor (observatie: fiecare rol are toate posibilitatile enumerate la cele de deasupra sa)
  - user nelogat (poate vizualiza toate paginile)
  - user logat (poate rezerva camere + poate modifica/sterge propriile rezervari si poate adauga review-uri hotelurilor + poate modifica/sterge propriile review-uri)
  - agent (poate adauga hoteluri + poate modifica/sterge propriile hoteluri, poate sterge/modifica orice rezervare)
  - admin (poate adauga/modifica/sterge orice - inclusiv tari)


### MUST: (7p) structura de baza (CRUD standard)  :heavy_check_mark:
- (1p) tari	
- (1p) hoteluri     
- (1p) review-uri 					
- (2p) camere
- (2p) rezervari


### MUST: (10p) interfata  :heavy_check_mark:
- cate (2p) pt fiecare unitate de baza din lista de la punctul anterior


### SHOULD: (5p) filtrare dupa descrierea hotelului :heavy_check_mark:
- pot fi bifate o serie de cerinte cautate (specificatii din descriere etc)


### SHOULD: (3p) cautare personalizata pentru a afisa camerele disponibile :heavy_check_mark:
- tara
- perioada (check-in, check-out)
- numarul persoanelor pentru care se va face rezervarea


### SHOULD: (2p) verificarea perioadei la crearea unei rezervari :heavy_check_mark:
- doar daca sunt locuri libere intre check-in si check-out, rezervarea este adaugata in baza de date
		

### SHOULD: (4p) istoric al rezervarilor anterioare :heavy_check_mark:
- orice utilizator poate vedea o lista cu toate rezervarile sale


### NICE: (10p) transferul unor rezervari de la un user la altul 
cumparatori care vor sa renunte la rezervare, pe langa varianta de a anula (astfel vor pierde toti banii), 
	ei pot pune pe site un anunt: aceasta devine oferta pentru alti utilizatori si le apare atunci cand deschid descrierea hotelului (astfel )
- (5p) modificarea CRUD-ului 
- (5p) aceste oferte vor fi primele:
  - (2p) pe pagina la recomandari
  - (3p) pe pagina cu hoteluri dupa cautare daca se potrivesc cu campurile selectate

##### ~ 34/44p

