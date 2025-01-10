import psycopg2, configparser, os

def execute_sql_from_file(cursor, file_name):
    cursor.execute(open(file_name, 'r').read())
    return cursor
    
def get_conn(db, user, passw):
    return psycopg2.connect(
        database=db,
        host='localhost',
        user=user,
        password=passw,
        port='5432'
    )

def get_config(file_name):
    config = configparser.ConfigParser()
    config.read(file_name)
    return (config.get('DB', 'username'), config.get('DB', 'password'))

#ini contains db username and pass
db_login = get_config('config.ini')
conn = get_conn('GameData', db_login[0], db_login[1])
cursor = conn.cursor()
cursor.execute

for file in os.listdir('testcases'):
    filename = os.fsdecode(file)
    if filename.endswith(".sql"):
        try:
            execute_sql_from_file(cursor, 'testcases\\{0}'.format(filename))
            print('Query successful, expected error. Testcase: {0} failed'.format(filename))
        except Exception as e:
            print(e)
            print('Testcase: {} passed.'.format(filename))
            conn.commit()
    else:
        continue

conn.commit()
conn.close()