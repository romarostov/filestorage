import pandas as pd
import sklearn
from sklearn.cross_validation import cross_val_score
from sklearn.ensemble.gradient_boosting import GradientBoostingClassifier
import numpy as np
import time
import datetime
from sklearn.cross_validation import KFold
from sklearn.metrics import roc_auc_score
from sklearn.preprocessing import StandardScaler
from sklearn.linear_model import LogisticRegression

data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\DSUPoseweniya.csv' )


data = pd.read_csv('./Fizkult/Data/contracs.csv' ,skiprows=1, index_col='kodklienta')


data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\contracs.csv' ,skiprows=0, index_col='kodklienta')
dataframe1 = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\contracs.csv' ,skiprows=0)

data2 = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\contracs2.csv' ,skiprows=1, index_col='kodklienta')
data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\contracs2.csv' ,skiprows=1, index_col='kodklienta')

Y=data['prodlilsya'].as_matrix()

data=data.drop(['kodkontrakta','prodlilsya','datanachalasrokadejstviya','dataokonchaniyasrokadejstviya'],1)
data=data.drop(['kodkontrakta','datanachalasrokadejstviya','dataokonchaniyasrokadejstviya'],1)






scaler=StandardScaler()
scaler.fit(data)
X=scaler.transform(data)


cv = KFold(Y.size, n_folds=5, shuffle=True)


#0.1

for C_coef in np.power(10.0, np.arange(-5, 6)):
    start_time = datetime.datetime.now()    
    clf=LogisticRegression(C=C_coef)    
    scores = cross_val_score(clf, X, Y, cv=cv,scoring='roc_auc')
    print ('C %s Score %s Time elapsed:%s' % (C_coef,scores.mean(), (datetime.datetime.now() - start_time)))

#C 1e-05 Score 0.91069640188 Time elapsed:0:00:00.136000
#C 0.0001 Score 0.912354016016 Time elapsed:0:00:00.086000
#C 0.001 Score 0.930120925498 Time elapsed:0:00:00.098000
#C 0.01 Score 0.948525080044 Time elapsed:0:00:00.130000
#C 0.1 Score 0.948889459841 Time elapsed:0:00:00.120000
#C 1.0 Score 0.948874882244 Time elapsed:0:00:00.135000
#C 10.0 Score 0.948874603381 Time elapsed:0:00:00.123000
#C 100.0 Score 0.948873255532 Time elapsed:0:00:00.125000
#C 1000.0 Score 0.94887298598 Time elapsed:0:00:00.127000
#C 10000.0 Score 0.948873255858 Time elapsed:0:00:00.123000
#C 100000.0 Score 0.948873255858 Time elapsed:0:00:00.124000

data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\DSUPoseweniya2.csv')
data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\DSUPoseweniya2.csv')
data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\Communications2.csv')
data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Mart\Data\Communications2.csv')

#�������� ���������� �������
#C 1e-05 Score 0.925360598232 Time elapsed:0:00:00.479000
#C 0.0001 Score 0.935195954937 Time elapsed:0:00:00.101000
#C 0.001 Score 0.951630419106 Time elapsed:0:00:00.115000
#C 0.01 Score 0.957875394291 Time elapsed:0:00:00.146000
#C 0.1 Score 0.957513867355 Time elapsed:0:00:00.182000
#C 1.0 Score 0.957379079509 Time elapsed:0:00:00.234000
#C 10.0 Score 0.957368575161 Time elapsed:0:00:00.250000
#C 100.0 Score 0.957367504029 Time elapsed:0:00:00.211000
#C 1000.0 Score 0.957367773581 Time elapsed:0:00:00.220000
#C 10000.0 Score 0.957367773581 Time elapsed:0:00:00.196000
#C 100000.0 Score 0.957367773581 Time elapsed:0:00:00.192000

#�������� ����� ���������� ������������

#C 1e-05 Score 0.925574091675 Time elapsed:0:00:00.210000
#C 0.0001 Score 0.936493667335 Time elapsed:0:00:00.124000
#C 0.001 Score 0.953278450151 Time elapsed:0:00:00.145000
#C 0.01 Score 0.959795642252 Time elapsed:0:00:00.227000
#C 0.1 Score 0.95935233281 Time elapsed:0:00:00.226000
#C 1.0 Score 0.959177018271 Time elapsed:0:00:00.239000
#C 10.0 Score 0.959149537339 Time elapsed:0:00:00.233000
#C 100.0 Score 0.959148193854 Time elapsed:0:00:00.236000
#C 1000.0 Score 0.959147924705 Time elapsed:0:00:00.233000
#C 10000.0 Score 0.959147655556 Time elapsed:0:00:00.235000
#C 100000.0 Score 0.959147655556 Time elapsed:0:00:00.232000

#C 1e-05 Score 0.92131089694 Time elapsed:0:00:00.149000
#C 0.0001 Score 0.935299247609 Time elapsed:0:00:00.166000
#C 0.001 Score 0.953433925161 Time elapsed:0:00:00.260000
#C 0.01 Score 0.960022209529 Time elapsed:0:00:00.264000
####C 0.1 Score 0.959667498776 Time elapsed:0:00:00.346000
#C 1.0 Score 0.959487588575 Time elapsed:0:00:00.403000
#C 10.0 Score 0.959449645693 Time elapsed:0:00:00.388000
#C 100.0 Score 0.959312969221 Time elapsed:0:00:00.479000
#C 1000.0 Score 0.959275310559 Time elapsed:0:00:00.517000
#C 10000.0 Score 0.959233339919 Time elapsed:0:00:00.531000
#C 100000.0 Score 0.959233610499 Time elapsed:0:00:00.531000


clf=LogisticRegression(C=0.1)    
clf.fit(X, Y)




from sklearn.externals import joblib
joblib.dump(clf, 'D:\Project\PythonApplication1\PythonApplication1\Fizkult\\fizkult.pkl') 
joblib.dump(scaler, 'D:\Project\PythonApplication1\PythonApplication1\Fizkult\\fizkultScaler.pkl') 


from sklearn.externals import joblib
clf = joblib.load('D:\Project\PythonApplication1\PythonApplication1\Fizkult\\fizkult.pkl')
scaler=joblib.load('D:\Project\PythonApplication1\PythonApplication1\Fizkult\\fizkultScaler.pkl') 

X=scaler.transform(data)
Y_test=clf.predict_proba(X)[:, 1]
roc_auc_score(Y,Y_test)


list(data.columns[data.count() <max(data.count())])

data['dlitelnostkontrakta']=data['dlitelnostkontrakta'].fillna(355)

data['prodlilsya']=Y_test

data['prodlilsya'].to_csv('finalFizkult.csv')




###################################
import math

from datetime import datetime

#class client:
#    TotalCommunications=0
#    TotalCommunicationsInLastMonth=0
#    TotalCommunicationsInPreviousMonth=0
clients={}

import pandas as pd
data = pd.read_csv('D:\Project\PythonApplication1\PythonApplication1\Fizkult\Data\Communications2.csv')

#com=pd.DataFrame(clients, index=["KodKlienta"], columns=[ 'TotalCommunications','TotalCommunicationsInLastMonth','TotalCommunicationsInPreviousMonth'])


lastMonthStart = datetime.strptime('1.11.2015 00:00:01', '%d.%m.%Y %H:%M:%S')
previousMonthStart = datetime.strptime('1.10.2015 00:00:01', '%d.%m.%Y %H:%M:%S')

for index, row in data.iterrows():
    if index % 1000 ==0:
        print index    
    kodclFloat=row['KodKlienta']
    if math.isnan(kodclFloat):
        continue
    kodclienta=int(kodclFloat)
    cl=clients.get(kodclienta)
    #print kodclienta
    if cl is None:
        cl=[0,0,0]
        clients[kodclienta]=cl
    cl[0]=cl[0]+1    
    visitTime=datetime.strptime(row['DataVzaimodeystviya'], '%d.%m.%Y %H:%M:%S')
    if visitTime > lastMonthStart:
        cl[1]=cl[1]+1    
    elif visitTime > previousMonthStart:
            cl[1]=cl[1]+1    

com=pd.DataFrame.from_dict(clients,orient='index',dtype=int)
com.columns=[ 'TotalCommunications','TotalCommunicationsInLastMonth','TotalCommunicationsInPreviousMonth']
com["kodklienta"]= com.index.values

cols = com.columns.tolist()

com.index




X=pd.concat([data,com],axis=1, join_axes=[data.index])
X=pd.concat([data,com],axis=1, join_axes=["kodklienta"])




#for kod, row in data.iterrows():
#    if math.isnan(kod):
#        continue    
#    cl=clients.get(kod)
#    if cl is None:
#        continue
#    data.xs(kod, copy = False)['TotalCommunications']=cl.TotalCommunications
#    #data.set_value(kod,"TotalCommunications",cl.TotalCommunications)
#    print kod,cl.TotalCommunications
    

#for kod, row in data.iterrows():    
#    data.xs(kod, copy = False)['TotalCommunications']=kod
#    #data.set_value(kod,"TotalCommunications",cl.TotalCommunications)
#    print kod

# data.ix[247824]["TotalCommunications"]=22

#data["TotalCommunications"]= 0
    
#for (kodKlienta, cl) in clients.items():
    
#    com.loc[kodclienta]=[cl.TotalCommunications,cl.TotalCommunicationsInLastMonth,cl.TotalCommunicationsInPreviousMonth]
#    print kodKlienta, cl.TotalCommunications




#for index, row in data.iterrows():
#    kodclFloat=row['KodKlienta']
#    if math.isnan(kodclFloat):
#        continue
#    kodclienta=int(kodclFloat)
#    if kodclienta is None:
#            continue
#    #print kodclienta
#    if kodclienta in com.index:
#        total=int(com.loc[kodclienta]["TotalCommunications"])
#        total=total+1;
#        com.loc[kodclienta]["TotalCommunications"]=total
#    else:
#        com.loc[kodclienta]=[1,0,0]



datetime.strptime('11.08.2015 20:32:31', '%d.%m.%Y %H:%M:%S')

        


def my_test2(vozrast):
    print vozrast
    if (vozrast == "New" or vozrast==""):
        return 1
    elif (vozrast == "Renew"):
        return 2
    elif (vozrast == "Ex"):
        return 3

def fixVozrast(vozrast):
    print vozrast
    if (vozrast == "Взрослые"):
        return 1
    elif (vozrast == "Kids"):
        return 2
    elif (vozrast == "Teens"):
        return 3

    
