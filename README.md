# Proiect MDS

realizat de: Dănescu Adela-Gabriela, Dîrțu Ecaterina, Mihăilescu Teodor, Pița Bogdan-Ioan 

## Epic story:

Booking.com este suprasolicitat, 
pentru binele cumparatorilor vrem sa ii ajutam 
sa gaseasca cele mai bune preturi intr-o noua aplicatie realizata de noi


## User stories:

### DONE must
- login standard (1p) + tipuri de useri cu atributiile lor - admin/agent/user (2p)

de ex: daca nu e user logat poate doar vedea cazarile, nu poate rezerva  (3p total)

### DONE must
structura de baza: (CRUD standard) (7p total)
- tari (tip categorii) (1p)	
- hoteluri 		(1p)    
- review-uri 	(1p)				
- camere	(2p)
- rezervare 	(2p)

### PARTIAL must
- interfata (10p total) - cate (2p) pt fiecare unitate de baza

### should
- filtru pe descrierea hotelului (3p)
	(zona, distante) 

### should
- filtru pe descrierea hotelului (2p)
	(obiective, activitati)

### should
- search pe baza de: pret min max, perioada, review-uri etc (2p)

### DONE should
- la rezervari, daca sunt locuri libere intre data start-data fin (1p)
		(task: fct cu frecventa) se face, daca nu, nu se poate

### should
- istoric al rezervarilor anterioare (pentru reutilizare de ex) (2p)

### nice
- recomandari pe baza unor rezervari anterioare / tari deja vizitate (3p)

### nice
cumparatori care nu mai vor sa mearga, in loc sa anuleze (se poate si asta - delete din crud), 
	pun inapoi pe site si acele cazari sunt cu reducere (~ resale pt altii)
 (o rezervare anulata devine oferta pt alti utilizatori 
	si le apare atunci cand dechid descrierea hotelului) (8p total)
- modificarea CRUD-ului (3p) 
- notificare daca ceva la resale era in recomandari (3p)
- hotelurile la care exista resale sa apara primele pe pagina (2p)

     ~ 41p
